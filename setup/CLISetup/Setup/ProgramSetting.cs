using System;
using static Setup.FileHandler;

namespace Setup {
	//HACK: This entire thing is a garbage winforms wrapper that I had to mangle here because I'm too lazy to change more than this.
	public class ProgramSetting<T> {
		public readonly string key;

		public ProgramSetting(string key) {
			this.key = key;
		}

		public void Set(T value) {
			Config config = Config.GetConfig();
			if (key == "TerrariaDiffCutoff")
				config.TerrariaDiffCutoff = (DateTime)(object)value;
			else if (key == "tModLoaderDiffCutoff")
				config.tModLoaderDiffCutoff = (DateTime)(object)value;
			Config.SetConfig(config);
		}

		public T Get() {
			Config config = Config.GetConfig();
			if (key == "TerrariaDiffCutoff")
				return (T)(object)config.TerrariaDiffCutoff;
			else if (key == "tModLoaderDiffCutoff")
				return (T)(object)config.tModLoaderDiffCutoff;
			else
				// This will error
				return (T)(object) new System.NotImplementedException();
		}
	}
}