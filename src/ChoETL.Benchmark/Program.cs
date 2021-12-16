﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChoETL;
using System.Linq;

namespace ChoETL.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChoETLFrxBootstrap.TraceLevel = TraceLevel.Off;
            ToDataTableFromDictionary();
        }

        static void ToDataTableFromDictionary()
        {
            var data = TestClassGenerator.GetTestEnumerable2(100000).Select(e => e.ToSimpleDictionary());

            for (int i = 0; i < 10; i++)
            {
                Stopwatch w = Stopwatch.StartNew();
                var dt = data.AsDataTable();
                dt.Print();
                break;
                w.Stop();
                w.ElapsedMilliseconds.ToString().Print();
            }
        }

        static void ToDataTableFromNullableValueType()
        {
            List<int?> list = new List<int?>
            {
                1,
                null,
                2
            };

            var dt = list.AsDataTable();
            dt.Print();
        }

        static void ToDataTableFromValueType()
        {
            List<string> list = new List<string>
            {
                "Tom",
                "Mark",
            };

            var dt = list.AsDataTable();
            dt.Print();
        }

        static void ToDataTableTest1()
        {
            var data = TestClassGenerator.GetTestEnumerable(100000);

            for (int i = 0; i < 10; i++)
            {
                Stopwatch w = Stopwatch.StartNew();
                var dt = data.AsDataTable();
                //dt.Print();
                //break;
                w.Stop();
                w.ElapsedMilliseconds.ToString().Print();
            }
        }
    }
}
