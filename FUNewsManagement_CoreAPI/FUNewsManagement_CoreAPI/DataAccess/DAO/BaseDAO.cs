using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HoangNV_SE1930_A01_BE.DataAccess.Context;

namespace HoangNV_SE1930_A01_BE.DataAccess.DAO;

public abstract class BaseDAO
{
    private static string? _connectionString;
    private static readonly object _connLock = new object();

    protected FunewsManagementContext CreateDbContext()
    {
        if (_connectionString == null)
        {
            lock (_connLock)
            {
                if (_connectionString == null)
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                    _connectionString = config.GetConnectionString("MyCnn");
                    
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        // Fallback to CurrentDirectory
                        var fallbackConfig = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .Build();
                        _connectionString = fallbackConfig.GetConnectionString("MyCnn");
                    }
                }
            }
        }

        var optionsBuilder = new DbContextOptionsBuilder<FunewsManagementContext>();
        if (!string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
        return new FunewsManagementContext(optionsBuilder.Options);
    }
}
