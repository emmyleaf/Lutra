using System.Collections.Generic;

namespace Lutra.Utility.Glide
{
    public class Tween
    {
        [Flags]
        public enum RotationUnit
        {
            Degrees,
            Radians
        }

        #region Callbacks
        private Easer ease;
        private Action begin, update, complete;
        #endregion

        #region Timing
        public bool Paused { get; private set; }
        private float Delay, repeatDelay;
        private readonly float Duration;

        private float time;
        #endregion

        private bool firstUpdate;
        private int repeatCount, timesRepeated;
        private GlideLerper.Behavior behavior;

        private readonly List<GlideInfo> vars;
        private readonly List<GlideLerper> lerpers;
        private readonly List<object> start, end;
        private readonly Dictionary<string, int> varHash;
        private readonly Tweener Tweener;

        /// <summary>
        /// The time remaining before the tween ends or repeats.
        /// </summary>
        public float TimeRemaining => Duration - time;

        /// <summary>
        /// A value between 0 and 1, where 0 means the tween has not been started and 1 means that it has completed.
        /// </summary>
        public float Completion
        {
            get { var c = time / Duration; return c < 0 ? 0 : (c > 1 ? 1 : c); }
            set => time = value * Duration;
        }

        /// <summary>
        /// Whether the tween is currently looping.
        /// </summary>
        public bool Looping => repeatCount != 0;

        /// <summary>
        /// The object this tween targets. Will be null if the tween represents a timer.
        /// </summary>
        public object Target { get; private set; }

        internal Tween(object target, float duration, float delay, Tweener parent)
        {
            Target = target;
            Duration = duration;
            Delay = delay;
            Tweener = parent;

            firstUpdate = true;

            varHash = [];
            vars = [];
            lerpers = [];
            start = [];
            end = [];
            behavior = GlideLerper.Behavior.None;
        }

        internal void AddLerp(GlideLerper lerper, GlideInfo info, object from, object to)
        {
            varHash.Add(info.PropertyName, vars.Count);
            vars.Add(info);

            start.Add(from);
            end.Add(to);

            lerpers.Add(lerper);
        }

        internal void Update(float elapsed)
        {
            if (firstUpdate)
            {
                firstUpdate = false;

                var i = vars.Count;
                while (i-- > 0)
                {
                    lerpers[i]?.Initialize(start[i], end[i], behavior);
                }
            }
            else
            {
                if (Paused)
                    return;

                if (Delay > 0)
                {
                    Delay -= elapsed;
                    if (Delay > 0)
                        return;
                }

                if (time == 0 && timesRepeated == 0 && begin != null)
                    begin();

                time += elapsed;
                float setTimeTo = time;
                float t = time / Duration;
                bool doComplete = false;

                if (time >= Duration)
                {
                    if (repeatCount != 0)
                    {
                        setTimeTo = 0;
                        Delay = repeatDelay;
                        timesRepeated++;

                        if (repeatCount > 0)
                            --repeatCount;

                        if (repeatCount < 0)
                            doComplete = true;
                    }
                    else
                    {
                        time = Duration;
                        t = 1;
                        Tweener.Remove(this);
                        doComplete = true;
                    }
                }

                if (ease != null)
                    t = ease(t);

                int i = vars.Count;
                while (i-- > 0)
                {
                    if (vars[i] != null)
                        vars[i].Value = lerpers[i].Interpolate(t, vars[i].Value, behavior);
                }

                time = setTimeTo;

                //	If the timer is zero here, we just restarted.
                //	If reflect mode is on, flip start to end
                if (time == 0 && behavior.HasFlag(GlideLerper.Behavior.Reflect))
                    Reverse();

                update?.Invoke();

                if (doComplete && complete != null)
                    complete();
            }
        }

        #region Behavior
        /// <summary>
        /// Apply target values to a starting point before tweening.
        /// </summary>
        /// <param name="values">The values to apply, in an anonymous type ( new { prop1 = 100, prop2 = 0} ).</param>
        /// <returns>A reference to this.</returns>
        public Tween From(object values)
        {
            var props = values.GetType().GetProperties();
            for (int i = 0; i < props.Length; ++i)
            {
                var property = props[i];
                var propValue = property.GetValue(values, null);

                if (varHash.TryGetValue(property.Name, out int index))
                {
                    //	if we're already tweening this value, adjust the range
                    start[index] = propValue;
                }

                //	if we aren't tweening this value, just set it
                var info = new GlideInfo(Target, property.Name, true);
                info.Value = propValue;
            }

            return this;
        }

        /// <summary>
        /// Set the easing function.
        /// </summary>
        /// <param name="ease">The Easer to use.</param>
        /// <returns>A reference to this.</returns>
        public Tween Ease(Easer ease)
        {
            this.ease = ease;
            return this;
        }

        /// <summary>
        /// Set a function to call when the tween begins (useful when using delays). Can be called multiple times for compound callbacks.
        /// </summary>
        /// <param name="callback">The function that will be called when the tween starts, after the delay.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnBegin(Action callback)
        {
            if (begin == null) begin = callback;
            else begin += callback;
            return this;
        }

        /// <summary>
        /// Set a function to call when the tween finishes. Can be called multiple times for compound callbacks.
        /// If the tween repeats infinitely, this will be called each time; otherwise it will only run when the tween is finished repeating.
        /// </summary>
        /// <param name="callback">The function that will be called on tween completion.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnComplete(Action callback)
        {
            if (complete == null) complete = callback;
            else complete += callback;
            return this;
        }

        /// <summary>
        /// Set a function to call as the tween updates. Can be called multiple times for compound callbacks.
        /// </summary>
        /// <param name="callback">The function to use.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnUpdate(Action callback)
        {
            if (update == null) update = callback;
            else update += callback;
            return this;
        }

        /// <summary>
        /// Enable repeating.
        /// </summary>
        /// <param name="times">Number of times to repeat. Leave blank or pass a negative number to repeat infinitely.</param>
        /// <returns>A reference to this.</returns>
        public Tween Repeat(int times = -1)
        {
            repeatCount = times;
            return this;
        }

        /// <summary>
        /// Set a delay for when the tween repeats.
        /// </summary>
        /// <param name="delay">How long to wait before repeating.</param>
        /// <returns>A reference to this.</returns>
        public Tween RepeatDelay(float delay)
        {
            repeatDelay = delay;
            return this;
        }

        /// <summary>
        /// Sets the tween to reverse every other time it repeats. Repeating must be enabled for this to have any effect.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Reflect()
        {
            behavior |= GlideLerper.Behavior.Reflect;
            return this;
        }

        /// <summary>
        /// Swaps the start and end values of the tween.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Reverse()
        {
            int i = vars.Count;
            while (i-- > 0)
            {
                var s = start[i];
                var e = end[i];

                //	Set start to end and end to start
                start[i] = e;
                end[i] = s;

                lerpers[i].Initialize(e, s, behavior);
            }

            return this;
        }

        /// <summary>
        /// Whether this tween handles rotation.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Rotation(RotationUnit unit = RotationUnit.Degrees)
        {
            behavior |= GlideLerper.Behavior.Rotation;
            behavior |= (unit == RotationUnit.Degrees) ? GlideLerper.Behavior.RotationDegrees : GlideLerper.Behavior.RotationRadians;

            return this;
        }

        /// <summary>
        /// Whether tweened values should be rounded to integer values.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Round()
        {
            behavior |= GlideLerper.Behavior.Round;
            return this;
        }
        #endregion

        #region Control
        /// <summary>
        /// Cancel tweening given properties.
        /// </summary>
        /// <param name="properties"></param>
        public void Cancel(params string[] properties)
        {
            var canceled = 0;
            for (int i = 0; i < properties.Length; ++i)
            {
                if (!varHash.TryGetValue(properties[i], out int index))
                    continue;

                varHash.Remove(properties[i]);
                vars[index] = null;
                lerpers[index] = null;
                start[index] = null;
                end[index] = null;

                canceled++;
            }

            if (canceled == vars.Count)
                Cancel();
        }

        /// <summary>
        /// Remove tweens from the tweener without calling their complete functions.
        /// </summary>
        public void Cancel()
        {
            Tweener.Remove(this);
        }

        /// <summary>
        /// Assign tweens their final value and remove them from the tweener.
        /// </summary>
        public void CancelAndComplete()
        {
            time = Duration;
            update = null;
            Tweener.Remove(this);
        }

        /// <summary>
        /// Set tweens to pause. They won't update and their delays won't tick down.
        /// </summary>
        public void Pause()
        {
            Paused = true;
        }

        /// <summary>
        /// Toggle tweens' paused value.
        /// </summary>
        public void PauseToggle()
        {
            Paused = !Paused;
        }

        /// <summary>
        /// Resumes tweens from a paused state.
        /// </summary>
        public void Resume()
        {
            Paused = false;
        }
        #endregion
    }
}
