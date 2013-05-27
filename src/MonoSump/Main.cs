using System;
using System.IO.Ports;
using Earlz.MonoSump.Core;
using Newtonsoft.Json.Linq;

namespace Earlz.MonoSump
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("MonoSump -- Sump Logic Analyzer Client");

			var handler=new ArgumentHandler();
			var config=handler.ParseCommands(args);
			if(config==null)
			{
				return;
			}
  
			var conf=new SumpConfiguration();
			Console.WriteLine(conf.SaveToJson());


			using(var serial=new Serial(config.DeviceName, 115200))
			{

				var commander=new SumpCommander(serial);
				Console.WriteLine("Resetting device");
				commander.Reset();
				if(config.Identify)
				{
					Console.WriteLine("Device ID: "+commander.GetID());
				}

				//test it out

				var masks=new bool[32];
				masks[0]=true;
				commander.SetTriggerMasks(0, masks);
				var values=new bool[32];
				commander.SetTriggerValues(0, values);
				commander.SetTriggerConfigurations(0, new TriggerConfiguration(){Start=true, Level=0});
				commander.SetReadAndDelayCount(100, 100);
				var flags=new SumpFlags();
				flags.Filter=false;
				flags.InvertedClock=false;
				flags.ExternalClock=false;
				commander.SetFlags(flags);
				commander.SetDivider(commander.Clock/60);
				commander.Run();
				var data=commander.GetData(1000, 1000000, 200);
				Console.WriteLine("done with "+data.Count+" frames");
				foreach(var d in data)
				{
					foreach(var b in d)
					{
						if(b)
						{
							Console.Write("1");
						}else
						{
							Console.Write("0");
						}
					}
					Console.WriteLine("");
				}
				Console.WriteLine("Json:");
				Console.WriteLine(data.ToJson());
			}
		}

	}
}
