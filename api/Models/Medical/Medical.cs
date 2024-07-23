using System;
using System.Text.Json.Serialization;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Medical.Database
{
    public interface IMedicalDbTable
    {
        long Id { get; set; }
    }

    public interface IMedicalDbView
    {
    }

    public class Lab : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Lab_1", Order = 0)]
        public long Id { get; set; }

        [Unique(Name = "sqlite_autoindex_Lab_2", Order = 0)]
        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string Description { get; set; }

        public string Comment { get; set; }

    }

    public class Person : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Person_1", Order = 0)]
        public long Id { get; set; }

        [Unique(Name = "sqlite_autoindex_Person_2", Order = 0)]
        [NotNull]
        public string CNP { get; set; }

        [NotNull]
        public string Name { get; set; }

        public string Comment { get; set; }

        [JsonIgnore]
        public string LoginId { get; set; }

    }

    public class Test : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Test_1", Order = 0)]
        public long Id { get; set; }

        [NotNull]
        public long LabId { get; set; }

        [NotNull]
        public long PersonId { get; set; }

        [NotNull]
        public long TestTypeId { get; set; }

        [NotNull]
        public DateTime Date { get; set; }

        public double? Value { get; set; }

        public double? MinRefOverride { get; set; }

        public double? MaxRefOverride { get; set; }

        [NotNull]
        public string Description { get; set; }

        public string Comment { get; set; }

    }

    public class TestCategory : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_TestCategory_1", Order = 0)]
        public long Id { get; set; }

        [Unique(Name = "sqlite_autoindex_TestCategory_2", Order = 0)]
        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string Description { get; set; }

        public string Comment { get; set; }

    }

    public class TestType : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_TestType_1", Order = 0)]
        public long Id { get; set; }

        [NotNull]
        public long TestCategoryId { get; set; }

        [Unique(Name = "sqlite_autoindex_TestType_2", Order = 0)]
        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public double MinRef { get; set; }

        [NotNull]
        public double MaxRef { get; set; }

        [NotNull]
        public string Description { get; set; }

        public string Comment { get; set; }

    }

    public class TestDetail : IMedicalDbView
    {
        [NotNull]
        public long TestId { get; set; }

        [NotNull]
        public long PersonId { get; set; }

        [NotNull]
        public long TestTypeId { get; set; }

        [NotNull]
        public long TestCategoryId { get; set; }

        [NotNull]
        public long LabId { get; set; }

        [NotNull]
        public string PersonName { get; set; }

        [NotNull]
        public string PersonCode { get; set; }

        public string PersonComment { get; set; }

        [NotNull]
        public string TestCategoryCode { get; set; }

        [NotNull]
        public string TestCategoryDescription { get; set; }

        [NotNull]
        public string TestTypeCode { get; set; }

        [NotNull]
        public string TestTypeDescription { get; set; }

        [NotNull]
        public double MinRef { get; set; }

        [NotNull]
        public double MaxRef { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public DateTime Date { get; set; }

        public double? Value { get; set; }

        public double? MinRefOverride { get; set; }

        public double? MaxRefOverride { get; set; }

    }

    public class TestTypeDetail : IMedicalDbView
    {
        [NotNull]
        public long TestTypeId { get; set; }

        [NotNull]
        public long TestCategoryId { get; set; }

        [NotNull]
        public string TestCategoryCode { get; set; }

        [NotNull]
        public string TestCategoryDescription { get; set; }

        [NotNull]
        public string TestTypeCode { get; set; }

        [NotNull]
        public string TestTypeDescription { get; set; }

        [NotNull]
        public double MinRef { get; set; }

        [NotNull]
        public double MaxRef { get; set; }

    }
}
