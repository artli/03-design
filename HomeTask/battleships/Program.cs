using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Conventions;


namespace battleships
{
	public class Program
	{
		private static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: {0} <ai.exe>", Process.GetCurrentProcess().ProcessName);
				return;
			}
			var aiPath = args[0];

            var kernel = new StandardKernel();
            kernel.Bind(x => x
                .FromThisAssembly()
                .SelectAllClasses()
                .BindAllInterfaces());
            kernel.Bind<Settings>().ToSelf().WithConstructorArgument("settings.txt");
            kernel.Bind<IAiFactory>().ToFactory();
            kernel.Bind<IGameFactory>().ToFactory();
            var tester = kernel.Get<AiTester>();

			if (File.Exists(aiPath))
				tester.TestSingleFile(aiPath);
			else
				Console.WriteLine("No AI exe-file " + aiPath);
		}
	}
}