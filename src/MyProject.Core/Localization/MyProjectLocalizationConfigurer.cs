using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace MyProject.Localization
{
	public static class MyProjectLocalizationConfigurer
	{
		public static void Configure(ILocalizationConfiguration localizationConfiguration)
		{

			//localizationConfiguration.Languages.Add(new LanguageInfo("vi", "Tiếng Việt", "VN"));
			//localizationConfiguration.Languages.Add(new LanguageInfo("en", "English", "US"));
			localizationConfiguration.Sources.Add(
					new DictionaryBasedLocalizationSource(MyProjectConsts.LocalizationSourceName,
							new XmlEmbeddedFileLocalizationDictionaryProvider(
									typeof(MyProjectLocalizationConfigurer).GetAssembly(),
									"MyProject.Localization.SourceFiles"
							)
					)
			);
		}
	}
}
