using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    this is a state driven class that will allow us to modify the race elements for any specific reason

    one of the game's rules is to have a way to modify the race automatically if the user answers incorrectly.

    when the user starts the race, we can automatically modify the elements at a given time.

    Race will pass in several of the game's main properties:  track, vehicle and environment

    We can access global props via Persistent Model
    
    STATES

    stopCarAtTime - stops the car and ends the race at a specific time, will use this for incorrect answers
    slowCar - decreases the vehicle's speed .  At some point in time, it will end the race

*/
public class RaceInjectorHandler
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

    public RaceInjectorHandler()
    {
        //Debug.Log("RaceInjectorHandler");

        events = new List<MyEvent>();

        CurrentState = State.INIT;
    }

    public void Initialize(Race raceObject)
    {
        //Debug.Log("RaceInjectorHandler.Initialize()");

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
        Debug.Log("RaceInjectorHandler.AddTimeEvent(" + startTime + ")");
        // StartEventTime = startTime;

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
                /*if (elapsedTime > 5)
                {
                    Debug.Log("RaceInjectorHandler.elapsedTime: " + elapsedTime);
                    CurrentState = State.TIMER_END;
                    CurrentEvent = Event.SLOW_DOWN;
                }*/

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
                Debug.Log("Event.SLOW_DOWN");
                //UIManager.Instance.soundManager.PlayTooSlow(0.065f);
                race.Vehicle.Speed = 10;
                CurrentState = State.END_EVENT;
                break;

            case Event.BREAKDOWN:

                break;

            case Event.FORCE_STOP:
                Debug.Log("Event.FORCE_STOP");
                ForceStop();
                break;
        }
    }

    // force stop will stop the vehicle and show wrong answer feedback 
    // on alert close, we remove all elements running
    // game manager, race and game screen are all removed
    private void ForceStop()
    {
        DebugLog.Trace("ForceStop.Instance.ChallengeTime: " + PersistentModel.Instance.ChallengeTime);
        DebugLog.Trace("race.Time: " + race.Time);

        PersistentModel.Instance.ClockIsStopped = true;

        //UIManager.Instance.soundManager.PlayTooSlow(0.065f);
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
