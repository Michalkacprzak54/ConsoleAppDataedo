namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportData(string fileToImport)
        {
            List<ImportedObject> importedObjects = new List<ImportedObject>();
            var streamReader = new StreamReader(fileToImport);
            var importedLines = new List<string>();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if(!string.IsNullOrWhiteSpace(line))
                {
                    var values = line.Split(';');;

                    // Sprawdź, czy values zawiera wystarczającą liczbę elementów
                    if (values.Length >= 7)
                    {
                        var importedObject = new ImportedObject
                        {
                            Type = values[0].Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper(),
                            Name = values[1].Trim().Replace(" ", "").Replace(Environment.NewLine, ""),
                            Schema = values[2].Trim().Replace(" ", "").Replace(Environment.NewLine, ""),
                            ParentName = values[3].Trim().Replace(" ", "").Replace(Environment.NewLine, ""),
                            ParentType = values[4].Trim().Replace(" ", "").Replace(Environment.NewLine, ""),
                            DataType = values[5],
                            IsNullable = values[6] == "1"
                        };
                        importedObjects.Add(importedObject);
                    }
                    
                }
                
            }

            CountChildren(importedObjects);
            PrintData(importedObjects);


            Console.ReadLine();
        }
        public void CountChildren(IEnumerable<ImportedObject> importedObjects)
        {
            foreach (var importedObject in importedObjects)
            {
                importedObject.NumberOfChildren = importedObjects.Count(obj =>
                    obj.ParentType == importedObject.Type && obj.ParentName == importedObject.Name);
            }
        }
        public void PrintData(IEnumerable<ImportedObject> importedObjects)
        {
            var databases = importedObjects.Where(obj => obj.Type.ToUpper() == "DATABASE");

            foreach (var database in databases)
            {
                var tables = importedObjects.Where(obj => obj.ParentType.ToUpper() == database.Type && obj.ParentName == database.Name);

                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                foreach (var table in tables)
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    var columns = importedObjects.Where(obj => obj.ParentType.ToUpper() == table.Type && obj.ParentName == table.Name);

                    foreach (var column in columns)
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }


        }

    }


}
