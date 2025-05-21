using System;

namespace ET;

[AttributeUsage(AttributeTargets.Class)]
public class DBVersionAttribute : BaseAttribute
{
    public DBVersionEnum DBVersion;

    public DBVersionAttribute(DBVersionEnum dbVersion)
    {
        this.DBVersion = dbVersion;
    }
}