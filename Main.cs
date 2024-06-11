using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace RandomWallets
{
    public class Main : Script
    {
        private readonly Random _random = new Random();
        private readonly float _spawnRange;
        private readonly float _spawnChance;
        private readonly float[] _spawnChanceMultipliers;
        private readonly bool _highlightWallets;
        private readonly int _minCash;
        private readonly int _maxCash;

        public Main()
        {
            // read settings
            Interval = Settings.GetValue("SPAWNING", "MS_BETWEEN_ATTEMPTS", 30000);
            _spawnRange = Settings.GetValue("SPAWNING", "SPAWN_RANGE", 30.0f);
            _spawnChance = Settings.GetValue("SPAWNING", "SPAWN_CHANCE", 0.03f);
            _highlightWallets = Settings.GetValue("SPAWNING", "HIGHLIGHT_WALLETS", true);
            _minCash = Settings.GetValue("REWARD", "MIN_CASH", 80);
            _maxCash = Settings.GetValue("REWARD", "MAX_CASH", 400);

            // same order as ZONE::GET_ZONE_SCUMMINESS
            _spawnChanceMultipliers = new float[]
            {
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_POSH", 1.0f),
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_NICE", 1.0f),
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_ABOVE_AVERAGE", 1.0f),
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_BELOW_AVERAGE", 1.0f),
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_CRAP", 1.0f),
                Settings.GetValue("SPAWN_CHANCE_MULTIPLIERS", "ZONE_TYPE_SCUM", 1.0f)
            };

            // event handlers
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Vector3 basePos = Game.Player.Character.Position.Around(_spawnRange);
            int scumminess = Function.Call<int>(Hash.GET_ZONE_SCUMMINESS, Function.Call<int>(Hash.GET_ZONE_AT_COORDS, basePos.X, basePos.Y, basePos.Z));
            if ((_spawnChance * _spawnChanceMultipliers[scumminess]) < _random.NextDouble())
            {
                return;
            }

            Vector3 spawnPos = World.GetSafeCoordForPed(basePos, true, 1 /* OnlyPavement */ | 4 /* NotInterior */ | 8 /* NotWater */);
            if (spawnPos == Vector3.Zero)
            {
                return;
            }

            int handle = Function.Call<int>(Hash.CREATE_NON_NETWORKED_AMBIENT_PICKUP, (int)PickupType.MoneyWallet, spawnPos.X, spawnPos.Y, spawnPos.Z, 8 /* SnapToGround */ | 16 /* OrientToGround */, _random.Next(_minCash, _maxCash), 0, false, false);
            if (_highlightWallets)
            {
                int blipHandle = Function.Call<int>(Hash.ADD_BLIP_FOR_ENTITY, handle);
                Function.Call(Hash.SET_BLIP_COLOUR, blipHandle, 2);
                Function.Call(Hash.SET_BLIP_SCALE, blipHandle, 0.7f);
                Function.Call(Hash.SET_BLIP_FLASHES, blipHandle, true);
                Function.Call(Hash.SET_BLIP_FLASH_INTERVAL, blipHandle, 250);
                Function.Call(Hash.SET_BLIP_FLASH_TIMER, blipHandle, 5000);
                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
            }
        }
    }
}
