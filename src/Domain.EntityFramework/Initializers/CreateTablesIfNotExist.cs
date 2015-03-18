﻿namespace Domain.EntityFramework.Initializers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Transactions;

    public class CreateTablesIfNotExist<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        private readonly string[] customCommands;

        private readonly string[] tablesToValidate;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="tablesToValidate">A list of existing table names to validate; null to don't validate table names</param>
        /// <param name="customCommands">A list of custom commands to execute</param>
        public CreateTablesIfNotExist(string[] tablesToValidate, string[] customCommands)
        {
            this.tablesToValidate = tablesToValidate;
            this.customCommands = customCommands;
        }

        public void InitializeDatabase(TContext context)
        {
            bool dbExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                dbExists = context.Database.Exists();
            }
            if (dbExists)
            {
                bool createTables;
                if (this.tablesToValidate != null &&
                    this.tablesToValidate.Length > 0)
                {
                    //we have some table names to validate
                    var existingTableNames =
                        new List<string>(
                            context.Database.SqlQuery<string>(
                                "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'"));
                    createTables =
                        !existingTableNames.Intersect(this.tablesToValidate, StringComparer.InvariantCultureIgnoreCase)
                                           .Any();
                }
                else
                {
                    //check whether tables are already created
                    var numberOfTables = 0;
                    foreach (
                        var t1 in
                            context.Database.SqlQuery<int>(
                                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE' "))
                    {
                        numberOfTables = t1;
                    }

                    createTables = numberOfTables == 0;
                }

                if (createTables)
                {
                    //create all tables
                    var dbCreationScript = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript();
                    context.Database.ExecuteSqlCommand(dbCreationScript);

                    //Seed(context);
                    context.SaveChanges();

                    if (this.customCommands != null &&
                        this.customCommands.Length > 0)
                    {
                        foreach (var command in this.customCommands)
                        {
                            context.Database.ExecuteSqlCommand(command);
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("No database instance");
            }
        }
    }
}