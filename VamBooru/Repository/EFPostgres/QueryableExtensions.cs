using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace VamBooru.Repository.EFPostgres
{
	#if(DEBUG)
	// https://github.com/aspnet/EntityFrameworkCore/issues/6482
	public static class QueryableExtensions
	{
		private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.Single(x => x.Name == "_queryCompiler");
		private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
		private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_queryModelGenerator");
		private static readonly FieldInfo DatabaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
		private static readonly PropertyInfo DependenciesProperty = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

		public static string ToSql<TEntity>(this IQueryable<TEntity> queryable)
			where TEntity : class
		{
			var queryCompiler = (IQueryCompiler)QueryCompilerField.GetValue(queryable.Provider);
			var queryModelGenerator = (IQueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
			var queryModel = queryModelGenerator.ParseQuery(queryable.Expression);
			var database = DatabaseField.GetValue(queryCompiler);
			var queryCompilationContextFactory = ((DatabaseDependencies)DependenciesProperty.GetValue(database)).QueryCompilationContextFactory;
			var queryCompilationContext = queryCompilationContextFactory.Create(false);
			var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
			modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
			return modelVisitor.Queries.Join(Environment.NewLine + Environment.NewLine);
		}
	}
	#endif
}
