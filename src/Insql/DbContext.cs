using Dapper;
using Insql.Mappers;
using Insql.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Insql
{
    public class DbContext : IDisposable
    {
        private readonly object _lock = new object();

        private readonly IInsqlModel _model;

        private readonly DbContextOptions _options;

        private IDbSession _session;

        public virtual IDbSession Session => this.GetSession();

        public IInsqlModel Model => this._model;

        public IDbDialect Dialect => this._options.Dialect;

        public DbContext(DbContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.ConfigureOptions(options);

            if (options.Resolver == null)
            {
                throw new ArgumentNullException(nameof(options.Resolver));
            }
            if (options.SessionFactory == null)
            {
                throw new ArgumentNullException(nameof(options.SessionFactory));
            }
            if (options.Dialect == null)
            {
                throw new ArgumentNullException(nameof(options.Dialect));
            }
            
            this._model = options.ServiceProvider.GetRequiredService<IInsqlModel>();

            this._options = options;
        }

        public virtual void Dispose()
        {
            if (this._session != null)
            {
                this._session.Dispose();
            }
        }

        public int Execute(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.Execute(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<int> ExecuteAsync(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteAsync(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public IDataReader ExecuteReader(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteReader(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<IDataReader> ExecuteReaderAsync(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteReaderAsync(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public object ExecuteScalar(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteScalar(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public T ExecuteScalar<T>(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteScalar<T>(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<object> ExecuteScalarAsync(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteScalarAsync(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<T> ExecuteScalarAsync<T>(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.ExecuteScalarAsync<T>(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public IEnumerable<T> Query<T>(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.Query<T>(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, true, this.Session.CommandTimeout);
        }

        public IEnumerable<object> Query(Type type, string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.Query(type, resolveResult.Sql, resolveResult.Param, this.Session.Transaction, true, this.Session.CommandTimeout);
        }

        public IEnumerable<dynamic> Query(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.Query(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, true, this.Session.CommandTimeout);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.QueryAsync<T>(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<IEnumerable<object>> QueryAsync(Type type, string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.QueryAsync(type, resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public Task<IEnumerable<dynamic>> QueryAsync(string sqlId, object sqlParam = null)
        {
            var resolveResult = this.Resolve(sqlId, sqlParam);

            return this.Session.Connection.QueryAsync(resolveResult.Sql, resolveResult.Param, this.Session.Transaction, this.Session.CommandTimeout);
        }

        public virtual ResolveResult Resolve(string sqlId, object sqlParam = null)
        {
            return this._options.Resolver.Resolve($"{sqlId}.{this._options.Dialect.Name}", sqlParam);
        }

        protected virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        private void ConfigureOptions(DbContextOptions options)
        {
            if (!options.IsConfigured)
            {
                var needConfigure = false;

                lock (this._lock)
                {
                    if (!options.IsConfigured)
                    {
                        options.IsConfigured = true;

                        needConfigure = true;
                    }
                }

                if (needConfigure)
                {
                    this.OnConfiguring(new DbContextOptionsBuilder(options));
                }
            }
        }

        private IDbSession GetSession()
        {
            if (this._session == null)
            {
                lock (this._lock)
                {
                    if (this._session == null)
                    {
                        this._session = this._options.SessionFactory.CreateSession();
                    }
                }
            }

            return this._session;
        }
    }
}
