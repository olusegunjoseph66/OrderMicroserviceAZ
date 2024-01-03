using Microsoft.Extensions.Options;
using Order.Application.Configurations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.SerilogService
{
    public class OrderLogger
    {
        private readonly SerilogConfiguration _serilog;
        public OrderLogger(IOptions<SerilogConfiguration> serilog)
        {
            _serilog = serilog.Value;
        }   

        public void LogRequest(string message, bool isError = false)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(
                _serilog.accountlogger,
                outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}",
                fileSizeLimitBytes: 1_000_000,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            if(isError) Log.Error(message); else Log.Information(message);

        }
    }
}
