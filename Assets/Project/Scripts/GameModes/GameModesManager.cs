using System;
using Project.Scripts.GameModes.Abstracts;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.GameModes
{
    public class GameModesManager : MonoBehaviourSingleton<GameModesManager>
    {
        [SerializeField] private AGameMode startGameMode;
        
        
        private AGameMode currentGameMode;
        
        
        private void Start()
        {
            if(startGameMode != null)
                LaunchGameMode(startGameMode);
        }
        
        
        public async void LaunchGameMode(AGameMode gameMode)
        {
            try
            {
                if(currentGameMode != null)
                    throw new InvalidOperationException("Game mode is already running.");

                currentGameMode = gameMode ?? throw new ArgumentNullException(nameof(gameMode), "Game mode cannot be null.");
            
                currentGameMode.Initialize();

                await currentGameMode.Update();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if(currentGameMode)
                    currentGameMode.Cleanup();
                
                currentGameMode = null;
            }
        }
    }
}