using FluentMigrator.Runner.VersionTableInfo;

namespace WareHouse.Migrations;

public class VersionTable : IVersionTableMetaData
{
    public bool OwnsSchema => true;
    public string SchemaName => "public";
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string DescriptionColumnName => "description";
    public string AppliedOnColumnName => "applied_on";
    public string UniqueIndexName => "uc_version_info_unique"; // Измененное имя

    public bool CreateWithPrimaryKey => false;
    public object? ApplicationContext { get; set; }
}