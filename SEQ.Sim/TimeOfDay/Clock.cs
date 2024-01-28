
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class Clock : AsyncScript, ICvarListener
    {
        public static Clock S;
        public string TimescaleCvar;
        public string MinuteCvar;
        public string HourCvar;
        public string DayCvar;
        public Action OnMinute;
        public Action OnHour;
        public Action OnDay;
        public float BulletTimeScale = 1f;

        float Timescale = 60f;
        public float WaitMinutes = .1f;

        public float GetSecondsPerHour()
        {
            return WaitMinutes * 60f;
        }

        public void OnValueChanged()
        {
            if (Cvars.TryGet<float>(TimescaleCvar, out var scale))
            {
                Timescale = scale;
                WaitMinutes = 1f / scale;
            }
        }

        public override async Task Execute()
        {
            S = this;
            if (!string.IsNullOrWhiteSpace(TimescaleCvar))
            {
                if (!Cvars.Listeners.Entries.ContainsKey(TimescaleCvar))
                {
                    Cvars.Listeners.Entries[TimescaleCvar] = new List<CvarListenerInfo>();

                }
                Cvars.Listeners.Entries[TimescaleCvar].Add(new CvarListenerInfo
                {
                    Listener = this,
                    OnValueChanged = OnValueChanged,
                });

                // OnValueChanged("", Cvars.Get(TimescaleCvar));
            }

            //  time = ((RootEffectRenderFeature)RootRenderFeature).CreateFrameCBufferOffsetSlot(GlobalKeys.Time.Name);

            while (true)
            {

                for (var i = 0; i < 60; i++)
                {
                    await Task.Delay((int)(WaitMinutes * 1000));
                    Time.clockTime = i + 60 * Minutes + 60 * 60 * Hours; // + 60 * 60 * 60 * 12 * Days;
                    MoveSun();
                }

                HandleRollOvers();
            }
        }

        void MoveSun()
        {
            var minuteFract = (Minutes / 60f);
            //var asFraction = (Hours / 12f) + (Minutes / 60f);
            var hoursFract = Hours + minuteFract;
            TryUpdateTimeCvar(hoursFract, false);
            if (hoursFract > 7 && hoursFract < 17)
            {
                if (hoursFract < 8)
                {
                    Sun.Intensity = MathUtil.Lerp(NightIntensity, DayIntensity, minuteFract);
                    AmbientDayLight.Intensity = minuteFract * AmbientDayIntensity;
                    AmbientNightLight.Intensity = (1 - minuteFract) * AmbientNightIntensity;
                    Background.Intensity = minuteFract;
                }
                else if (hoursFract > 16)
                {
                    Sun.Intensity = MathUtil.Lerp(DayIntensity, NightIntensity, minuteFract);
                    AmbientDayLight.Intensity = (1 - minuteFract) * AmbientDayIntensity;
                    AmbientNightLight.Intensity = minuteFract * AmbientNightIntensity;
                    Background.Intensity = 1 - minuteFract;
                }
                else
                {
                    Sun.Intensity = DayIntensity;
                    AmbientDayLight.Intensity = AmbientDayIntensity;
                    AmbientNightLight.Intensity = 0;
                    Background.Intensity = 1;
                }

                //  Transform.RotationEulerXYZ = new Vector3(
                TargetSunAngle = MathUtil.Lerp(-90f, 88.694f,  (hoursFract - 7) / 10) * MathUtil.Deg2Rad;
                //    ,
                //  0, 0);
            }
            else
            {
                Sun.Intensity = NightIntensity;
                AmbientNightLight.Intensity = AmbientNightIntensity;
                AmbientDayLight.Intensity = 0;
                Background.Intensity = 0;
            }
        }

        public float TargetSunAngle;

        public void ApplyTime()
        {
            var minuteFract = (Minutes / 60f);
            //var asFraction = (Hours / 12f) + (Minutes / 60f);
            var hoursFract = Hours + minuteFract;
            TryUpdateTimeCvar(hoursFract, true);
            MoveSun();
            OnHour?.Invoke();
            OnMinute?.Invoke();
            OnDay?.Invoke();
        }

        void TryUpdateTimeCvar(float hoursFract, bool force)
        {
            var shouldBeDay = hoursFract > 7.5 && hoursFract < 4.5;
            if (force || (shouldBeDay && !isDay) || (!shouldBeDay && isDay))
            {
                isDay = shouldBeDay;
                Cvars.Set("time", isDay ? "day" : "night");
            }
        }

        bool isDay;
        public LightComponent Sun;

        public LightComponent AmbientDayLight;
        public float AmbientDayIntensity = 0.05f;
        public LightComponent AmbientNightLight;
        public float AmbientNightIntensity = 0.05f;
        public BackgroundComponent Background;
        public float DayIntensity = 1;
        public float SunriseIntensity;
        public float NightIntensity = 0;
        public float SunsetIntensity;

        public void HandleRollOvers()
        {
            var h = false;
            var d = false;
            Cvars.TryGet<int>(MinuteCvar, out Minutes);
            Cvars.TryGet<int>(HourCvar, out Hours);
            Cvars.TryGet<int>(DayCvar, out Days);

            Minutes += 1;

            if (Minutes > 59)
            {
                Cvars.Set(MinuteCvar, "0");
                Minutes = 0;
                Hours += 1;
                Cvars.Set(HourCvar, (Hours).ToString());
                h = true;
            }
            
            Cvars.Set(MinuteCvar, Minutes.ToString());

            if (Hours > 23)
            {
                Cvars.Set(HourCvar, "0");
                Hours = 0;
                Days += 1;
                Cvars.Set(DayCvar, (Days).ToString());
                d = true;
            }
            OnMinute?.Invoke();
       //     ClockFeatureKeys.Minutes.set
            if (h)
            {
                OnHour?.Invoke();
            }
            if (d)
            {
                OnDay?.Invoke();
            }
        }

        public int Minutes;
        public int Hours;
        public int Days;

        public int HoursAndDays => Days * 24 + Hours;

        public string GetClockTime()
        {
            return $"{Hours:D2}:{Minutes:D2}";
            //    if (Cvars.TryGet<int>(MinuteCvar, out var min)
            //      && Cvars.TryGet<int>(HourCvar, out var hr))
            //{
            // return $"{hr:D2}:{min:D2}";
            //   var pm = hr > 11;
            /*       if (SystemPrefsMgr.Get<bool>(SystemPref.Use24Hour))
                   {
                   }
                   else
                   {
                       if (hr == 0)
                           hr = 24;
                       if (hr > 12)
                           hr -= 12;
                       return pm
                           ? $"{hr}:{min:D2} pm"
                           : $"{hr}:{min:D2} am";
                   }*/
            //   }
            //  return "00:00";
        }
    }
}

/*
public enum ClockType
{
    RealTime,
    GameTime,
}
public static class Clocks
{
    public static Clock Game;
    public static Clock Real;

    public static void Register(Clock keeper)
    {
        switch (keeper.Type)
        {
            case ClockType.RealTime:
                Real = keeper;
                return;
            default:
                Game = keeper;
                return;
        }
    }
}
    /*
    public class ClockTimeManager
    {
        public double AsDouble { get; private set; }
        public int AsSeconds { get; private set; }
        double Carryover;
        public void Increment(float dt)
        {
            Carryover += dt;
            if (Carryover > 1)
            {
                AsSeconds ++;
                Carryover -= 1;
            }
            AsDouble = Carryover + AsSeconds;
        }

        public void Increment(double dt)
        {
            Carryover += dt;
            if (Carryover > 1)
            {
                AsSeconds++;
                Carryover -= 1;
            }
            AsDouble = Carryover + AsSeconds;
        }

        public void Set(int seconds)
        {
            AsSeconds = seconds;
            AsDouble = seconds;
            Carryover = 0;
        }
    }

    public class DateTimeManager
    {
        public DateTime DateTime = new DateTime();
        public void Increment(double dt)
        {
            DateTime = DateTime.AddSeconds(dt);
        }

        public void Set(int year, int month, int day, int hour, int min, int sec)
        {
            DateTime = new DateTime(year, month, day, hour, min, sec);
        }
    }

    public class Clock
    {
        public static Clock Inst;
        public DateTimeManager GameTime = new DateTimeManager();
        public ClockTimeManager SimTime = new ClockTimeManager();
        public ClockTimeManager RealTime = new ClockTimeManager();

        public double GameTimescale = 1f;
        public Clock()
        {
            Inst = this;
        }

   //     [Command("Sets counter for time elapsed in Unity sim seconds")]
        public static void SetSimtime(int seconds)
        {
            if (Inst != null)
                Inst.SimTime.Set(seconds);
            else
                Logger.Log(Channel.General, Priority.Error, $"COuld not set time: no clock inst");
        }

    //    [Command("Sets counter for time elapsed in real unscaled seconds")]
        public static void SetRealtime(int seconds)
        {
            if (Inst != null)
                Inst.RealTime.Set(seconds);
            else
                Logger.Log(Channel.General, Priority.Error, $"COuld not set time: no clock inst");
        }

    //    [Command("wrapper for Time.timeScale")]
        public static void Timescale(float timescale)
        {
            if (Inst != null)
            {
                Time.timeScale = timescale;
            }
            else
                Logger.Log(Channel.General, Priority.Error, $"COuld not set time: no clock inst");
        }

    //    [Command("Sets game clock timescale")]
        public static void TimescaleGame(double timescale)
        {
            Inst.GameTimescale = timescale;
        }

     //   [Command("Sets game clock date")]
        public static void Date(int year, int month, int day, int hour, int min, int sec)
        {
            Inst.GameTime.Set(year, month, day, hour, min, sec);
        }

        public static void Pause()
        {
            Time.timeScale = 0;
        }

        public static void Unpause()
        {
            Time.timeScale = 1;
        }

        public void OnUpdate()
        {
            GameTime.Increment(Time.deltaTime * Inst.GameTimescale);
            SimTime.Increment(Time.deltaTime);
            RealTime.Increment(Time.unscaledDeltaTime);
        }
    }
}
*/