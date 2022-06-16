using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace CSVFileReader
{
    public class CsvFileReader : ICsvFileReader
    {
        public Task<ICsvFile> ReadFileAsync(string fileName)
        {
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

        public CsvFile() { }

        #region Get Methods
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

        public void LoadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
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
                                        //If the row has less columns then the header row add a blank field to the row
                                        row.AddField(_Columns[x], "");
                                    }
                                }
                                _Rows.Add(row);
                            }
                        }

                        FileStatus = "File has been loaded.";
                    }
                }
            }
            else
            {
                FileStatus = "File does not exist. Please check the file and try again.";
            }
        }

        private void SetColumnHeader(string[] headerRow)
        {
            List<string> header = new List<string>();

            foreach (string column in headerRow)
            {
                if (header.Contains(column))
                {
                    //If the header is duplicated attempt to make it unique before adding it.
                    header.Add(column + header.Count(x => x == column).ToString());
                }
                else if(string.IsNullOrEmpty(column.Trim()))
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

        public string[] CsvSplitLine(string line)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] returnColumns = CSVParser.Split(line);
            CleanRow(ref returnColumns);
            return returnColumns;
        }

        /// <summary>
        /// Removes double quotes from each column of the given row
        /// </summary>
        /// <param name="rawRow"></param>
        private void CleanRow(ref string[] rawRow)
        {
            for (int i = 0; i < rawRow.Length; i++)
            {
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
