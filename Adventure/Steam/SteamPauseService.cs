using Adventure.Services;
using Steamworks;
using System;

namespace Adventure.Steam;

class SteamPauseService
{
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

    public SteamPauseService(PauseService pauseService)
    {
        m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        PauseService = pauseService;
    }

    public PauseService PauseService { get; }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        PauseService.Live = pCallback.m_bActive == 0;
    }
}
