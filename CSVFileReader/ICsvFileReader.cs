using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSVFileReader
{
    public interface ICsvFileReader
    {
        Task<ICsvFile> ReadFileAsync(string fileName);
    }
    public interface ICsvFile : IEnumerable<ICsvRow>
    {
        int RowsCount { get; }
        IEnumerable<string> Columns { get; }
        ICsvRow this[int rowIndex] { get; }
    }
    public interface ICsvRow : IEnumerable<(string column, string value)>
    {
        string this[string columnName] { get; }
    }
}
