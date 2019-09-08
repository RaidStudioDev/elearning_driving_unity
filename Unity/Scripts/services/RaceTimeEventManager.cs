using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Time event manager - add events during race
*/
public class RaceTimeEventManager
{
    public enum State
    {
        INIT,
        IDLE,
        TIMER_START,
        TIMER_RUNNING,
        TIMER_END,
        START_EVENT,
        RUNNING_EVENT,
        END_EVENT
    }

    private State PreviousState { get; set; }
    public State CurrentState { get; private set; }

    public enum Event
    {
        FORCE_STOP,
        SLOW_DOWN,
        BREAKDOWN
    }
    public Event CurrentEvent { get; private set; }

    class MyEvent
    {
        public Event name;
        public float startTime;
        public float duration;
        public bool completed;
    }
    
    private float StartEventTime { get; set; }
    private float CurrentTimeCount { get; set; }


    private float startTime = 0f;
    private float elapsedTime = 0f;

    private List<MyEvent> events;
    public int CurrentEventIndex { get; private set; }

    private Race race;

    public RaceTimeEventManager()
    {
        events = new List<MyEvent>();

        CurrentState = State.INIT;
    }

    public void Initialize(Race raceObject)
    {
        race = raceObject;

        ResetEvents();

        CurrentState = State.IDLE;
    }

    private void ResetEvents()
    {
        events.Clear();

        StartEventTime = 0f;
        CurrentTimeCount = 0f;
    }

    public void AddTimeEvent(Event eventName, float startTime, float duration = -1f, bool completed = false)
    {
        DebugLog.Trace("RaceInjectorHandler.AddTimeEvent(" + startTime + ")");
        
        MyEvent raceEvent = new MyEvent()
        {
            name = eventName,
            startTime = startTime,
            duration = duration,
            completed = completed
        };

        events.Add(raceEvent);
    }

    public void Start()
    {
        CurrentState = State.TIMER_START;
    }

    public void Update(float gT)
    {
        switch (CurrentState)
        {
            case State.INIT:

                break;

            case State.IDLE:

                break;

            case State.TIMER_START:
                startTime = CurrentTimeCount = gT;
                CurrentState = State.TIMER_RUNNING;
                break;

            case State.TIMER_RUNNING:
                // start counting the time and check if we are done
                CurrentTimeCount += gT;
                elapsedTime = CurrentTimeCount - startTime;

                for (int i = 0; i < events.Count; i++)
                {
                    if (elapsedTime > events[i].startTime && !events[i].completed)
                    {
                        CurrentState = State.TIMER_END;
                        CurrentEventIndex = i;
                        break;
                    }
                }
           
                CurrentTimeCount += gT;
                break;

            case State.TIMER_END:
                CurrentState = State.START_EVENT;
                break;

            case State.START_EVENT:
                CurrentState = State.RUNNING_EVENT;
                StartEvent(events[CurrentEventIndex].name);
                break;

            case State.RUNNING_EVENT:
                
                break;

            case State.END_EVENT:
                events[CurrentEventIndex].completed = true;

                // restart or resume event timer
                CurrentState = State.TIMER_START; // || State.TIMER_RUNNING
                break;
        }

    }

    public void StartEvent(Event evtName)
    {
        switch (evtName)
        {
            case Event.SLOW_DOWN:
                // DebugLog.Trace("Event.SLOW_DOWN");
                UIManager.Instance.soundManager.PlaySound("PlayTooSlow");
                race.Vehicle.Speed = 10;
                CurrentState = State.END_EVENT;
                break;

            case Event.BREAKDOWN:

                break;

            case Event.FORCE_STOP:
                // DebugLog.Trace("Event.FORCE_STOP");
                ForceStop();
                break;
        }
    }

    // force stop will stop the vehicle and show wrong answer feedback 
    // on alert close, we remove all elements running
    // game manager, race and game screen are all removed
    private void ForceStop()
    {
        PersistentModel.Instance.ClockIsStopped = true;
        UIManager.Instance.soundManager.PlaySound("PlayTooSlow", 0.2f);

        race.ForceCompleted();
        race.StopVehicle();

        string feedback = PersistentModel.Instance.TireOptionSelectedData.feedback;

        // Show Popup
        OverlaySettings settings = new OverlaySettings
        {
            body = feedback
        };
        UIManager.Instance.Overlay.ShowGameAlert(settings, OnInGameAlertClose);

        CurrentState = State.IDLE;
    }

    private void OnInGameAlertClose()
    {
        DebugLog.Trace("OnInGameAlertClose");
        DebugLog.Trace("PersistentModel.Instance.ChallengeTime: " + PersistentModel.Instance.ChallengeTime);
        DebugLog.Trace("race.Time: " + race.Time);

        // save time
        PersistentModel.Instance.ChallengeTime = race.Time;

        // remove game screen ui
        UIManager.Instance.RemoveCurrentScreen();

        // unloads race
        GameManager.Instance.Unload();

        // set to idle state
        CurrentState = State.IDLE;

        Remove();

        // go back to quiz
        UIManager.Instance.ShowScreen(UIManager.Screen.QUIZ_SCREEN);
    }

    public void UpdateEvent()
    {

    }

    public void Remove()
    {
        race = null;

        events.Clear();
    }
}
