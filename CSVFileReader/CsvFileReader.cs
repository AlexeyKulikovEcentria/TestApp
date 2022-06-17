using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

// review.
// code is not structured properly
// everything in the single file.

namespace CSVFileReader
{
    public class CsvFileReader : ICsvFileReader
    {
        // review.
        // separation of concerns violated
        // the reader delegates the reading operation to CsvFile class

        public Task<ICsvFile> ReadFileAsync(string fileName)
        {
            // review.
            // the async file reading is recommended here
            // see: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/using-async-for-file-access
            
            Task<ICsvFile> task = new Task<ICsvFile>(() => new CsvFile(fileName));
            return task;
        }
    }

    public class CsvFile : ICsvFile
    {
        private List<ICsvRow> _Rows = new List<ICsvRow>();
        private string[] _Columns = Array.Empty<string>();

        public CsvFile(string fileName)
        {
            LoadFile(fileName);
        }

        // review.
        // this constructor is never used and makes no sense

        public CsvFile() { }

        #region Get Methods

        // review.
        // better to use Enum for any kind of Status flags
        // or even switch to Exceptions approach (program may throw exception if error happened)
        // also the FileStatus is not available from ICsvFile interface

        public string FileStatus { get; set; }
        public ICsvRow this[int rowIndex]
        {
            get
            {
                return _Rows[rowIndex];
            }
        }

        public int RowsCount
        {
            get 
            {
                return _Rows.Count; 
            }
        }

        public IEnumerable<string> Columns
        {
            get
            {
                return _Columns;
            }
        }

        public IEnumerator<ICsvRow> GetEnumerator()
        {
            return _Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        // review.
        // separation of concerns violated
        // see above (comment on CsvFileReader class)
        // no necessary to set this method as Public

        public void LoadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                // review.
                // there is no handling of multi line values
                // see https://datatracker.ietf.org/doc/html/rfc4180
                // pt. 2.6 "Fields containing line breaks (CRLF), double quotes, and commas should be enclosed in double-quotes"

                string[] rawRows = File.ReadAllLines(fileName);
                //Sanity check
                if (rawRows.Length > 0)
                {
                    //Assume the first line is the header row
                    SetColumnHeader(CsvSplitLine(rawRows[0]));

                    //Only continue if there is more than 1 line and if there were columns found
                    if (rawRows.Length > 1 && _Columns.Length > 0)
                    {
                        for (int i = 1; i < rawRows.Length; i++)
                        {
                            //Don't add a new line if the line was blank
                            if (!string.IsNullOrEmpty(rawRows[i]))
                            {
                                string[] rowValues = CsvSplitLine(rawRows[i]);
                                //Finally add the row!
                                CsvRow row = new CsvRow();
                                for (int x = 0; x < _Columns.Length; x++)
                                {
                                    if (x < rowValues.Length)
                                    {
                                        row.AddField(_Columns[x], rowValues[x]);
                                    }
                                    else
                                    {
                                        // review.
                                        // by the spec. https://datatracker.ietf.org/doc/html/rfc4180
                                        // pt 2.4
                                        // Each line should contain the same number of fields throughout the file
                                        // better to raise a data format error

                                        //If the row has less columns then the header row add a blank field to the row
                                        row.AddField(_Columns[x], "");
                                    }
                                }
                                _Rows.Add(row);
                            }
                        }

                        FileStatus = "File has been loaded.";
                    }

                    // review.
                    // else { ...
                    // In this case the status will be null
                }
            }
            else
            {
                FileStatus = "File does not exist. Please check the file and try again.";
            }
        }

        private void SetColumnHeader(string[] headerRow)
        {
            // review.
            // O(N) linear complexity
            // better to use other data structure

            List<string> header = new List<string>();

            foreach (string column in headerRow)
            {
                // review.
                // Invalid implementation, the Task has additional requirement: Columns cannot duplicate
                // The program could raise an error here

                if (header.Contains(column))
                {
                    //If the header is duplicated attempt to make it unique before adding it.
                    header.Add(column + header.Count(x => x == column).ToString());
                }
                else if (string.IsNullOrEmpty(column.Trim()))
                {
                    //If there was a blank in the header
                    header.Add("Blank Column" + header.Count(x => x == "Blank Column").ToString());
                }
                else
                {
                    header.Add(column);
                }
            }
            _Columns = header.ToArray();

        }

        // review.
        // 1) no proper handling of quotes in the value
        // spec. https://datatracker.ietf.org/doc/html/rfc4180
        // pt. 2.7
        // example: "col1,col 2,"this is "" tricky"
        // 2) no proper handling of data format errors
        // 3) regular expression are pretty slow, better to use simple loop

        public string[] CsvSplitLine(string line)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] returnColumns = CSVParser.Split(line);
            CleanRow(ref returnColumns);
            return returnColumns;
        }

        // review.
        // ref keyword is not needed here

        /// <summary>
        /// Removes double quotes from each column of the given row
        /// </summary>
        /// <param name="rawRow"></param>
        private void CleanRow(ref string[] rawRow)
        {
            for (int i = 0; i < rawRow.Length; i++)
            {

                // review.
                // unnecessary trimming
                // spec. https://datatracker.ietf.org/doc/html/rfc4180
                // pt. 2.4 Spaces are considered part of a field and should not be ignored.

                string field = rawRow[i].Trim();
                if (field.Length >= 2)
                {
                    //Only remove quotations if the text starts and ends with double quotes
                    if (field.StartsWith('"') && field.EndsWith('"'))
                    {
                        //Remove double quotes or set to empty string
                        rawRow[i] = field.Length == 2 ? "" : field.Substring(1, field.Length - 2);
                    }
                    else
                        rawRow[i] = field;
                }
            }
        }
    }
    public class CsvRow : ICsvRow
    {
        // review.
        // Not needed to store Names
        // better to have a different data structure

        private List<(string ColumnName, string Value)> _Columns =  new List<(string, string)>();

        /// <summary>
        /// Adds a field to the Columns list
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public void AddField(string columnName, string value)
        {
            //Store column name as lower to ignore casing later on in the search
            _Columns.Add((columnName.ToLower(), value));
        }

        // review.
        // non optimal solution for files with a large amount of columns
        // O(N) complexity for each value getting operation
        // better choose other data structure

        public string this[string columnName]
        {
            get
            {
                //Convert given column name to HashCode to ignore casing.
                if (!string.IsNullOrEmpty(columnName.Trim()) 
                    && _Columns.Where(x => x.ColumnName == columnName.ToLower()).Count() > 0)
                {
                    return _Columns.Where(x => x.ColumnName == columnName.ToLower()).First().Value;
                }
                else
                {
                    // review.
                    // how can we discriminate if this is an Error or the value "Invalid Column"?
             
                    return "Invalid Column";
                }
            }
        }

        public IEnumerator<(string column, string value)> GetEnumerator()
        {
            return _Columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
