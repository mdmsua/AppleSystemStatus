using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AppleSystemStatus.Interceptors
{
    internal class TelemetryInterceptor : DbCommandInterceptor
    {
        private const string dependencyType = "SQL";

        private readonly TelemetryClient telemetryClient;

        public TelemetryInterceptor(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return result;
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, false);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, false);
            return Task.CompletedTask;
        }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return result;
        }

        public override Task<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return Task.FromResult(result);
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return result;
        }

        public override Task<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return Task.FromResult(result);
        }

        public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return result;
        }

        public override Task<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object result, CancellationToken cancellationToken = default)
        {
            telemetryClient.TrackDependency(dependencyType, eventData.Connection.DataSource, eventData.Command.CommandText, eventData.StartTime, eventData.Duration, true);
            return Task.FromResult(result);
        }
    }
}