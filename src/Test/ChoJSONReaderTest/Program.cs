﻿using ChoETL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoJSONReaderTest
{

    public class Message
    {
        public string Base
        {
            get;
            set;
        }
        public Dictionary<string, string> Rates
        {
            get;
            set;
        }
    }
    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    class Program
    {
        private static string EmpJSON = @"    
        [
          {
            ""Id"": 1,
            ""Name"": ""Raj"",
            ""Courses"": [ ""Math"", ""Tamil""],
            ""Dict"": {""key1"":""value1"",""key2"":""value2""}
          },
          {
            ""Id"": 2,
            ""Name"": ""Tom"",
          }
        ]
        ";

        private static string Stores = @"{
  'Stores': [
    'Lambton Quay',
    'Willis Street'
  ],
  'Manufacturers': [
    {
      'Name': 'Acme Co',
      'Products': [
        {
          'Name': 'Anvil',
          'Price': 50
        }
      ]
    },
    {
      'Name': 'Contoso',
      'Products': [
        {
          'Name': 'Elbow Grease',
          'Price': 99.95
        },
        {
          'Name': 'Headlight Fluid',
          'Price': 4
        }
      ]
    }
  ]}            ";

        static void Main(string[] args)
        {
            Sample2();
        }
        static void Sample2()
        {
            using (var csv = new ChoCSVWriter("sample2.csv") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.WithFirstLineHeader())
            {
                csv.Write(new ChoJSONReader("sample2.json") { TraceSwitch = ChoETLFramework.TraceSwitchOff }
                .WithField("Base")
                .WithField("Rates", fieldType: typeof(Dictionary<string, object>))
                .Select(m => ((Dictionary<string, object>)m.Rates).Select(r => new { Base = m.Base, Key = r.Key, Value = r.Value })).SelectMany(m => m)
                );
            }
        }

        static void Sample1()
        {
            using (var csv = new ChoCSVWriter("sample1.csv") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.WithFirstLineHeader())
            {
                csv.Write(new ChoJSONReader("sample1.json") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.Select(e => Flatten(e)));
            }
        }
        private static object[] Flatten(dynamic e)
        {
            List<object> list = new List<object>();
            list.Add(new { F1 = e.F1, F2 = e.F2, E1 = String.Empty, E2 = String.Empty, D1 = String.Empty, D2 = String.Empty });
            foreach (var se in e.F3)
            {
                if (se["E3"] != null)
                {
                    foreach (var de in se.E3)
                        list.Add(new { F1 = e.F1, F2 = e.F2, E1 = se.E1, E2 = se.E2, D1 = de.D1, D2 = de.D2 });
                }
                else
                    list.Add(new { F1 = e.F1, F2 = e.F2, E1 = se.E1, E2 = se.E2, D1 = String.Empty, D2 = String.Empty });
            }
            return list.ToArray();
        }
        static void JsonToXml()
        {
            using (var csv = new ChoXmlWriter("companies.xml") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.WithXPath("companies/company"))
            {
                csv.Write(new ChoJSONReader<Company>("companies.json") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.NotifyAfter(10000).Take(10).
                    SelectMany(c => c.Products.Touch().
                    Select(p => new { c.name, c.Permalink, prod_name = p.name, prod_permalink = p.Permalink })));
            }
        }

        static void JsonToCSV()
        {
            using (var csv = new ChoCSVWriter("companies.csv") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.WithFirstLineHeader())
            {
                csv.Write(new ChoJSONReader<Company>("companies.json") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.NotifyAfter(10000).Take(10).
                    SelectMany(c => c.Products.Touch().
                    Select(p => new { c.name, c.Permalink, prod_name = p.name, prod_permalink = p.Permalink })));
            }
        }

        static void LoadTest()
        {
            using (var p = new ChoJSONReader<Company>("companies.json") { TraceSwitch = ChoETLFramework.TraceSwitchOff }.NotifyAfter(10000))
            {
                p.Configuration.ColumnCountStrict = true;
                foreach (var e in p)
                    Console.WriteLine("overview: " + e.name);
            }

            //Console.WriteLine("Id: " + e.name);
        }

        public class Product
        {
            [ChoJSONRecordField]
            public string name { get; set; }
            [ChoJSONRecordField]
            public string Permalink { get; set; }
        }

        public class Company
        {
            [ChoJSONRecordField]
            public string name { get; set; }
            [ChoJSONRecordField]
            public string Permalink { get; set; }
            [ChoJSONRecordField(JSONPath = "$.products")]
            public Product[] Products { get; set; }
        }
        static void QuickLoad()
        {
            foreach (dynamic e in new ChoJSONReader("Emp.json"))
                Console.WriteLine("Id: " + e.Id + " Name: " + e.Name);
        }

        static void POCOTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new ChoJSONReader<EmployeeRec>(reader))
            {
                writer.WriteLine(EmpJSON);

                writer.Flush();
                stream.Position = 0;

                object rec;
                while ((rec = parser.Read()) != null)
                {
                    Console.WriteLine(rec.ToStringEx());
                }
            }
        }
        static void StorePOCOTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new ChoJSONReader<StoreRec>(reader).WithJSONPath("$.Manufacturers"))
            {
                writer.WriteLine(Stores);

                writer.Flush();
                stream.Position = 0;

                object rec;
                while ((rec = parser.Read()) != null)
                {
                    Console.WriteLine(rec.ToStringEx());
                }
            }
        }
        static void StorePOCONodeLoadTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var jparser = new JsonTextReader(reader))
            {
                writer.WriteLine(Stores);

                writer.Flush();
                stream.Position = 0;

                var config = new ChoJSONRecordConfiguration() { UseJSONSerialization = true };
                object rec;
                using (var parser = new ChoJSONReader<StoreRec>(JObject.Load(jparser).SelectTokens("$.Manufacturers"), config))
                {
                    while ((rec = parser.Read()) != null)
                    {
                        Console.WriteLine(rec.ToStringEx());
                    }
                }
            }
        }
        static void QuickLoadTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new ChoJSONReader(reader).WithJSONPath("$.Manufacturers").WithField("Name", fieldType: typeof(string)).WithField("Products", fieldType: typeof(Product[])))
            {
                writer.WriteLine(Stores);

                writer.Flush();
                stream.Position = 0;

                object rec;
                while ((rec = parser.Read()) != null)
                {
                    Console.WriteLine(rec.ToStringEx());
                }
            }
        }
        static void QuickLoadSerializationTest()
        {
            var config = new ChoJSONRecordConfiguration() { UseJSONSerialization = false };

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new ChoJSONReader(reader, config).WithJSONPath("$.Manufacturers"))
            {
                writer.WriteLine(Stores);

                writer.Flush();
                stream.Position = 0;

                object rec;
                while ((rec = parser.Read()) != null)
                {
                    Console.WriteLine(rec.ToStringEx());
                }
            }
        }
        public class EmployeeRec
        {
            public int Id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
            public string[] Courses
            {
                get;
                set;
            }
            public Dictionary<string, string> Dict
            {
                get;
                set;
            }
            public override string ToString()
            {
                return "{0}. {1}. Course Count: {2}. Dict Count: {3}".FormatString(Id, Name, Courses == null ? 0 : Courses.Length, Dict == null ? 0 : Dict.Count);
            }
        }
        public class ProductRec
        {
            public string Name
            {
                get;
                set;
            }
            public string Price
            {
                get;
                set;
            }
        }
        public class StoreRec
        {
            public string Name
            {
                get;
                set;
            }
            public ProductRec[] Products
            {
                get;
                set;
            }
            public override string ToString()
            {
                return "{0}. {1}.".FormatString(Name, Products == null ? 0 : Products.Length);
            }
        }
    }
}
