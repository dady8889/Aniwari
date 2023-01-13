using Aniwari.DAL.Nyaa;

namespace Aniwari.BL.Interfaces;

public interface INyaaService
{
    Task<List<NyaaAnime>> GetAnime(string title);
}
