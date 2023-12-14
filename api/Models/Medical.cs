using System;
using System.Text.Json.Serialization;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Medical.API
{

}


namespace ocpa.ro.api.Models.Medical.Database
{
    public interface IMedicalDbTable
    {
        Int64 Id { get; set; }
    }

    public class IMedicalDbView
    {
    }

    public class Lab : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Lab_1", Order = 0)]
        public Int64 Id { get; set; }

        [Unique(Name = "sqlite_autoindex_Lab_2", Order = 0)]
        [NotNull]
        public String Code { get; set; }

        [NotNull]
        public String Description { get; set; }

        public String Comment { get; set; }

    }

    public class Person : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Person_1", Order = 0)]
        public Int64 Id { get; set; }

        [Unique(Name = "sqlite_autoindex_Person_2", Order = 0)]
        [NotNull]
        public String CNP { get; set; }

        [NotNull]
        public String Name { get; set; }

        public String Comment { get; set; }

        [JsonIgnore]
        public String LoginId { get; set; }

    }

    public class Test : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_Test_1", Order = 0)]
        public Int64 Id { get; set; }

        [NotNull]
        public Int64 LabId { get; set; }

        [NotNull]
        public Int64 PersonId { get; set; }

        [NotNull]
        public Int64 TestTypeId { get; set; }

        [NotNull]
        public DateTime Date { get; set; }

        public Double? Value { get; set; }

        public Double? MinRefOverride { get; set; }

        public Double? MaxRefOverride { get; set; }

        [NotNull]
        public String Description { get; set; }

        public String Comment { get; set; }

    }

    public class TestCategory : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_TestCategory_1", Order = 0)]
        public Int64 Id { get; set; }

        [Unique(Name = "sqlite_autoindex_TestCategory_2", Order = 0)]
        [NotNull]
        public String Code { get; set; }

        [NotNull]
        public String Description { get; set; }

        public String Comment { get; set; }

    }

    public class TestType : IMedicalDbTable
    {
        [PrimaryKey, AutoIncrement]
        [Unique(Name = "sqlite_autoindex_TestType_1", Order = 0)]
        public Int64 Id { get; set; }

        [NotNull]
        public Int64 TestCategoryId { get; set; }

        [Unique(Name = "sqlite_autoindex_TestType_2", Order = 0)]
        [NotNull]
        public String Code { get; set; }

        [NotNull]
        public Double MinRef { get; set; }

        [NotNull]
        public Double MaxRef { get; set; }

        [NotNull]
        public String Description { get; set; }

        public String Comment { get; set; }

    }

    public class TestDetail : IMedicalDbView
    {
        [NotNull]
        public Int64 TestId { get; set; }

        [NotNull]
        public Int64 PersonId { get; set; }

        [NotNull]
        public Int64 TestTypeId { get; set; }

        [NotNull]
        public Int64 TestCategoryId { get; set; }

        [NotNull]
        public Int64 LabId { get; set; }

        [NotNull]
        public String PersonName { get; set; }

        [NotNull]
        public String PersonCode { get; set; }

        public String PersonComment { get; set; }

        [NotNull]
        public String TestCategoryCode { get; set; }

        [NotNull]
        public String TestCategoryDescription { get; set; }

        [NotNull]
        public String TestTypeCode { get; set; }

        [NotNull]
        public String TestTypeDescription { get; set; }

        [NotNull]
        public Double MinRef { get; set; }

        [NotNull]
        public Double MaxRef { get; set; }

        [NotNull]
        public String Description { get; set; }

        [NotNull]
        public DateTime Date { get; set; }

        public Double? Value { get; set; }

        public Double? MinRefOverride { get; set; }

        public Double? MaxRefOverride { get; set; }

    }

    public class TestTypeDetail : IMedicalDbView
    {
        [NotNull]
        public Int64 TestTypeId { get; set; }

        [NotNull]
        public Int64 TestCategoryId { get; set; }

        [NotNull]
        public String TestCategoryCode { get; set; }

        [NotNull]
        public String TestCategoryDescription { get; set; }

        [NotNull]
        public String TestTypeCode { get; set; }

        [NotNull]
        public String TestTypeDescription { get; set; }

        [NotNull]
        public Double MinRef { get; set; }

        [NotNull]
        public Double MaxRef { get; set; }

    }
}
