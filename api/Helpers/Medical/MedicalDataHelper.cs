﻿using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Generic;
using ocpa.ro.api.Models.Medical.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers.Medical
{
    public interface IMedicalDataHelper
    {
        List<T> AllOfType<T>() where T : class, IMedicalDbTable, new();
        List<TestTypeDetail> TestTypes(string categoryCode);
        Person Person(string loginId);
        List<TestDetail> Tests(int? id, int? pid, string cnp, string category, string type, DateTime? from, DateTime? to);
        int SaveMedicalRecord<T>(T record) where T : class, IMedicalDbTable, new();
        int DeleteMedicalRecord<T>(int id) where T : class, IMedicalDbTable, new();
    }

    public class MedicalDataHelper : IMedicalDataHelper
    {
        private readonly MedicalDB _mdb = null;

        static readonly JsonSerializerSettings _ss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        static readonly JsonSerializer _ser = JsonSerializer.Create(_ss);

        static readonly JsonMergeSettings _ms = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
        };

        public MedicalDataHelper(IWebHostEnvironment hostingEnvironment)
        {
            string dataFolder = Path.Combine(hostingEnvironment.ContentPath(), "Medical");
            _mdb = MedicalDB.Open(Path.Combine(dataFolder, "Medical.db3"), true);
        }

        public List<T> AllOfType<T>() where T : class, IMedicalDbTable, new()
        {
            return _mdb.Database.Table<T>().ToList();
        }

        public List<TestTypeDetail> TestTypes(string categoryCode)
        {
            categoryCode = (categoryCode ?? string.Empty).ToUpper();

            var types = _mdb.Database.Table<TestTypeDetail>()
                .Where(ttd => categoryCode == string.Empty || categoryCode == ttd.TestCategoryCode.ToUpper());

            if (!types.Any())
                throw new ExtendedException("ERROR_TEST_TYPE_NOT_FOUND");

            return types.ToList();
        }

        public Person Person(string loginId)
        {
            var persons = _mdb.Database.Table<Person>()
                .Where(p => p.LoginId == loginId);

            if (persons?.Count() > 0)
            {
                if (persons.Count() > 1)
                    throw new ExtendedException("MULTIPLE_PERSONS_FOUND");

                return persons.First();
            }

            throw new ExtendedException("ERROR_PERSON_NOT_FOUND");
        }

        public List<TestDetail> Tests(int? id, int? pid, string cnp, string category, string type, DateTime? from, DateTime? to)
        {
            if (cnp?.Length > 0)
                CnpValidator.Validate(cnp); // Will throw exception if CNP not valid

            int testId = id.GetValueOrDefault(-1);
            int personId = id.GetValueOrDefault();

            category = (category ?? string.Empty).ToUpper();
            type = (type ?? string.Empty).ToUpper();

            DateTime dtFrom = from.GetValueOrDefault(new DateTime(1900, 1, 1));
            DateTime dtTo = to.GetValueOrDefault(DateTime.Now.AddDays(1));

            var tests = _mdb.Database.Table<TestDetail>()
                .Where
                (td =>
                    (td.PersonId == personId || td.PersonCode == cnp) &&
                    (testId < 0 || testId == td.TestId) &&
                    (category == string.Empty || category == td.TestCategoryCode) &&
                    (type == string.Empty || type == td.TestTypeCode) &&
                    dtFrom <= td.Date &&
                    dtTo >= td.Date
                );

            if (tests?.Count() > 0)
                return tests.ToList();

            throw new ExtendedException("TESTS_NOT_FOUND");
        }

        public int SaveMedicalRecord<T>(T record) where T : class, IMedicalDbTable, new()
        {
            if (record.Id > 0)
            {
                T origRecord = _mdb.Database.Table<T>().FirstOrDefault(t => t.Id == record.Id);
                if (origRecord != null)
                {
                    var j1 = JObject.FromObject(origRecord, _ser);
                    var j2 = JObject.FromObject(record, _ser);
                    j1.Merge(j2, _ms);

                    var updateRecord = j1.ToObject<T>(_ser);

                    if (_mdb.Database.Update(updateRecord) > 0)
                        return (int)HttpStatusCode.OK;

                    throw new ExtendedException("ERROR_RECORD_NOT_UPDATED");
                }

                throw new ExtendedException("ERROR_UPDATE_RECORD_NOT_FOUND");
            }
            else
            {
                if (_mdb.Database.Insert(record) > 0)
                    return (int)HttpStatusCode.Created;

                throw new ExtendedException("ERROR_RECORD_NOT_INSERTED");
            }
        }

        public int DeleteMedicalRecord<T>(int id) where T : class, IMedicalDbTable, new()
        {
            T origRecord = _mdb.Database.Table<T>().FirstOrDefault(t => t.Id == id);
            if (origRecord != null)
            {
                if (_mdb.Database.Delete(origRecord) > 0)
                    return (int)HttpStatusCode.OK;

                throw new ExtendedException("ERROR_RECORD_NOT_DELETED");
            }

            throw new ExtendedException("ERROR_DELETE_RECORD_NOT_FOUND");
        }
    }


    public class MedicalDB
    {
        private SQLiteConnection _db = null;
        private readonly string _origPath = null;

        public SQLiteConnection Database => _db;

        public static MedicalDB Open(string path, bool write)
        {
            if (File.Exists(path))
                return new MedicalDB(path, write);

            return null;
        }

        private MedicalDB(string path, bool write)
        {
            _origPath = path;
            ReOpen(write);
        }

        private void ReOpen(bool write)
        {
            Close();
            if (!File.Exists(_origPath))
                throw new ExtendedException($"{_origPath} does not exist.");

            SQLiteOpenFlags openFlags = write ? SQLiteOpenFlags.ReadWrite : SQLiteOpenFlags.ReadOnly;
            _db = new SQLiteConnection(_origPath, openFlags);
        }

        public void Close()
        {
            _db?.Execute("VACUUM");
            _db?.Close();
            _db?.Dispose();
            _db = null;
        }

        public void SaveAndClose()
        {
            Close();
        }
    }
}
