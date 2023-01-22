using Aniwari.BL.Interfaces;
using Aniwari.DAL.Interfaces;
using Aniwari.DAL.Jikan;
using Aniwari.DAL.Storage;

namespace Aniwari.Managers;

public interface ITitleManager
{
    string GetPreferredAnimeTitle(ITitle anime);
    string GetPreferredAnimeTitle(int animeId);
}

public class TitleManager : ITitleManager
{
    private readonly ISettingsService _settingsService;
    private readonly IAnimeRepository _animeRepository;

    public TitleManager(ISettingsService settingsService, IAnimeRepository animeRepository)
    {
        _settingsService = settingsService;
        _animeRepository = animeRepository;
    }

    public string GetPreferredAnimeTitle(ITitle anime)
    {
        var animeTitle = anime.GetTitle(ConvertTitleType(_settingsService.GetStore().PreferredTitleLanguage));

        if (animeTitle == null)
            animeTitle = anime.GetDefaultTitle();

        if (animeTitle == null)
            throw new Exception($"Could not get preferred title for anime.");

        return animeTitle;
    }

    public string GetPreferredAnimeTitle(int animeId)
    {
        var anime = _animeRepository.GetAnimeById(animeId);

        if (anime == null)
            throw new Exception($"Could not find anime {animeId}.");

        return GetPreferredAnimeTitle(anime);
    }

    public static TitleType ConvertTitleType(PreferredTitleLanguage titleType)
    {
        switch (titleType)
        {
            case PreferredTitleLanguage.Romanized:
                return TitleType.Default;
            case PreferredTitleLanguage.English:
                return TitleType.English;
            case PreferredTitleLanguage.Japanese:
                return TitleType.Japanese;
        }

        return TitleType.Default;
    }
}
