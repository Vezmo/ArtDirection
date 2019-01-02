using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// Timer class.
/// </summary>
public class Timer
{

  public delegate void OnStartEvent();
  public delegate void OnRepeatEvent();
  public delegate void OnCompleteEvent();

  public TimerState timerState = TimerState.Stopped;

  public float m_currentTime = 0f;
  public bool m_useTargetTime = true;
  [SerializeField]
  private float m_targetTime = 0f;

  public float TargetTime
  {
    get
    {
      return m_targetTime;
    }
    set
    {
      m_targetTime = value;
    }
  }

  public int CurrentRepeats = 0;
  public int TargetRepeats = 1;
  public bool RepeatForever = false;

  public event OnStartEvent OnStart;
  public event OnRepeatEvent OnRepeat;
  public event OnCompleteEvent OnComplete;

  public Timer()
  {
    m_useTargetTime = false;
  }

  public Timer(float _targetTime)
  {
    Set(_targetTime);
  }

  public Timer(float _targetTime, int _targetRepeats)
  {
    Set(_targetTime, _targetRepeats);
  }

  public Timer(float _targetTime, bool _targetRepeats)
  {
    Set(_targetTime, _targetRepeats);
  }

  public virtual void Set(float _targetTime, int _targetRepeats = 1)
  {
    TargetTime = _targetTime;
    TargetRepeats = _targetRepeats;
    m_useTargetTime = (TargetTime >= 0 && TargetRepeats > 0);
  }

  public virtual void Set(float myTargetTime, bool myRepeatForever)
  {
    TargetTime = myTargetTime;
    RepeatForever = myRepeatForever;
    m_useTargetTime = (TargetTime > 0);
  }

  /// <summary>
  /// Starts the timer
  /// </summary>
  public virtual void Start()
  {
    timerState = TimerState.Playing;
    if (OnStart != null) OnStart();
  }

  /// <summary>
  /// Pauses the updating of the timer
  /// </summary>
  public virtual void Pause()
  {
    timerState = TimerState.Paused;
  }

  /// <summary>
  /// Stops the timer, resetting it.
  /// </summary>
  public virtual void Stop()
  {
    Reset();
    timerState = TimerState.Stopped;
  }

  /// <summary>
  /// Resets the time and repeat count. Does not change the play state.
  /// </summary>
  public virtual void Reset()
  {
    m_currentTime = 0;
    CurrentRepeats = 0;
  }

  /// <summary>
  /// Update the timer using the delta time.
  /// </summary>
  public virtual void Loop()
  {
    if (timerState == TimerState.Playing)
      Loop(Time.deltaTime);
  }

  /// <summary>
  /// Update the timer using a given delta time.
  /// </summary>
  public virtual void Loop(float _deltaTime)
  {
    if (timerState == TimerState.Playing)
    {
      UpdateTimer(_deltaTime);
    }
  }

  /// <summary>
  /// Returns the normalized time, between the range 0,1. Does not take repeats into account.
  /// </summary>
  public float GetNormalizedTime()
  {
    return m_currentTime / TargetTime;
  }

  /// <summary>
  /// Update the timer using a given delta time.
  /// </summary>
  protected virtual void UpdateTimer(float _deltaTime)
  {
    m_currentTime += _deltaTime;
    if (m_useTargetTime && m_currentTime > TargetTime)
    {
      ReachTargetTime();
    }
  }

  /// <summary>
  /// Called when the current time reaches the target time.
  /// </summary>
  protected virtual void ReachTargetTime()
  {
    CurrentRepeats++;
    if (CurrentRepeats < TargetRepeats || RepeatForever)
    {
      m_currentTime = 0;
      if (OnRepeat != null) OnRepeat();
    }
    else {
      Stop();
      if (OnComplete != null) OnComplete();
    }
  }

  public override string ToString()
  {
    return string.Format("{0}: State: {1}, Time: {2}, Repeats: {3}", GetType(), timerState, m_currentTime, CurrentRepeats);
  }

  public enum TimerState
  {
    Stopped = 0,
    Playing = 5,
    Paused = 10,
  }

}
