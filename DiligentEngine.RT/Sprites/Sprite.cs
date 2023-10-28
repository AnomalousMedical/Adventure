﻿using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Sprites
{
    public class SpriteFrameAttachment
    {
        public Vector3 translate;

        public static SpriteFrameAttachment FromFramePosition(float x, float y, float z, float width, float height)
        {
            float fx = x / (float)width;
            float fy = y / (float)height;

            fx = fx - 0.5f;
            fy = fy - 0.5f;
            fy *= -1f;

            return new SpriteFrameAttachment(new Vector3(fx, fy, z));
        }

        public SpriteFrameAttachment()
        {
            this.translate = Vector3.Zero;
        }

        public SpriteFrameAttachment(Vector3 translate)
        {
            this.translate = translate;
        }
    }

    public class SpriteFrame
    {
        public SpriteFrame()
        {

        }

        public SpriteFrame(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public List<SpriteFrameAttachment> Attachments { get; set; }
    }

    public class SpriteAnimation
    {
        public SpriteAnimation(long frameTime, params SpriteFrame[] frames)
        {
            this.frameTime = frameTime;
            this.frames = frames;
            this.duration = frameTime * frames.Length;
        }

        public long duration;
        public long frameTime;
        public SpriteFrame[] frames;
    }

    public class Sprite : ISprite
    {
        private Dictionary<String, SpriteAnimation> animations;
        private SpriteAnimation current;
        private String currentName;
        private long frameTime;
        private long duration;
        private int frame;

        public Vector3 BaseScale { get; set; }

        private static readonly Dictionary<String, SpriteAnimation> defaultAnimation = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation(1, new SpriteFrame[]{ new SpriteFrame()
                {
                    Left = 0f,
                    Top = 0f,
                    Right = 1f,
                    Bottom = 1f,
                    Attachments = new List<SpriteFrameAttachment>(){ new SpriteFrameAttachment() }
                } })
            }
        };

        public Sprite()
            : this(defaultAnimation)
        {

        }

        /// <summary>
        /// Constructor to supply an animation. The animations can be shared between multiple instances.
        /// </summary>
        /// <param name="animations"></param>
        public Sprite(Dictionary<String, SpriteAnimation> animations)
        {
            this.animations = animations;
            SetAnimation(animations.Keys.First());
        }

        public void RandomizeFrameTime()
        {
            //This is converted to int, but its still random
            //Anything into the long territory will be a pretty long animation anyway
            frameTime = Random.Shared.Next(0, (int)duration);
        }

        public void SetAnimation(String animationName)
        {
            if (animationName == currentName)
            {
                return;
            }

            currentName = animationName;

            if (!animations.TryGetValue(animationName, out current))
            {
                var first = animations.First();
                current = first.Value;
                currentName = first.Key;
            }
            frameTime = 0;
            duration = current.duration;
        }

        public void Update(Clock clock)
        {
            frameTime += clock.DeltaTimeMicro;
            frameTime %= duration;
            frame = (int)((float)frameTime / duration * current.frames.Length);
        }

        public SpriteFrame GetCurrentFrame()
        {
            return current.frames[frame];
        }

        public String CurrentAnimationName => currentName;

        public int FrameIndex => frame;

        public IReadOnlyDictionary<String, SpriteAnimation> Animations => animations;
    }

    public class FrameEventSprite : ISprite
    {
        private Dictionary<String, SpriteAnimation> animations;
        private SpriteAnimation current;
        private String currentName;
        private long frameTime;
        private long duration;
        private int frame;

        public Vector3 BaseScale { get; set; } = Vector3.ScaleIdentity;

        public event Action<FrameEventSprite> AnimationChanged;
        public event Action<FrameEventSprite> FrameChanged;

        public FrameEventSprite()
            : this(new Dictionary<string, SpriteAnimation>()
            {
                { "default", new SpriteAnimation(1, new SpriteFrame[]{ new SpriteFrame()
                    {
                        Left = 0f,
                        Top = 0f,
                        Right = 1f,
                        Bottom = 1f
                    } })
                }
            })
        {

        }

        public FrameEventSprite(Dictionary<String, SpriteAnimation> animations)
        {
            this.animations = animations;
            SetAnimation(animations.Keys.First());
        }

        public void SetAnimation(String animationName)
        {
            if(animationName == currentName)
            {
                return;
            }

            currentName = animationName;

            if (!animations.TryGetValue(animationName, out current))
            {
                var first = animations.First();
                current = first.Value;
                currentName = first.Key;
            }
            frameTime = 0;
            duration = current.duration;
            frame = 0;

            AnimationChanged?.Invoke(this);
        }

        public void Update(Clock clock)
        {
            var oldFrame = frame;
            frameTime += clock.DeltaTimeMicro;
            frameTime %= duration;
            frame = (int)((float)frameTime / duration * current.frames.Length);
            if(FrameChanged != null && frame != oldFrame)
            {
                FrameChanged.Invoke(this);
            }
        }

        public void RandomizeFrameTime()
        {
            var oldFrame = frame;
            //This is converted to int, but its still random
            //Anything into the long territory will be a pretty long animation anyway
            frameTime = Random.Shared.Next(0, (int)duration);
            if (FrameChanged != null && frame != oldFrame)
            {
                FrameChanged.Invoke(this);
            }
        }

        public SpriteFrame GetCurrentFrame()
        {
            return current.frames[frame];
        }

        public String CurrentAnimationName => currentName;

        public int FrameIndex => frame;

        public IReadOnlyDictionary<String, SpriteAnimation> Animations => animations;
    }

    public class KeepTimeSprite : ISprite
    {
        private Dictionary<String, SpriteAnimation> animations;
        private SpriteAnimation current;
        private String currentName;
        private long frameTime;
        private long duration;
        private int frame;

        public Vector3 BaseScale { get; set; }

        private static readonly Dictionary<String, SpriteAnimation> defaultAnimation = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation(1, new SpriteFrame[]{ new SpriteFrame()
                {
                    Left = 0f,
                    Top = 0f,
                    Right = 1f,
                    Bottom = 1f,
                    Attachments = new List<SpriteFrameAttachment>(){ new SpriteFrameAttachment() }
                } })
            }
        };

        public KeepTimeSprite()
            : this(defaultAnimation)
        {

        }

        /// <summary>
        /// Constructor to supply an animation. The animations can be shared between multiple instances.
        /// </summary>
        /// <param name="animations"></param>
        public KeepTimeSprite(Dictionary<String, SpriteAnimation> animations)
        {
            this.animations = animations;
            SetAnimation(animations.Keys.First());
        }

        public void RandomizeFrameTime()
        {
            //This is converted to int, but its still random
            //Anything into the long territory will be a pretty long animation anyway
            frameTime = Random.Shared.Next(0, (int)duration);
        }

        public void SetAnimation(String animationName)
        {
            if (animationName == currentName)
            {
                return;
            }

            currentName = animationName;

            if (!animations.TryGetValue(animationName, out current))
            {
                var first = animations.First();
                current = first.Value;
                currentName = first.Key;
            }
            duration = current.duration;
            frameTime = frameTime % duration;
        }

        public void Update(Clock clock)
        {
            frameTime += clock.DeltaTimeMicro;
            frameTime %= duration;
            frame = (int)((float)frameTime / duration * current.frames.Length);
        }

        public SpriteFrame GetCurrentFrame()
        {
            return current.frames[frame];
        }

        public String CurrentAnimationName => currentName;

        public int FrameIndex => frame;

        public IReadOnlyDictionary<String, SpriteAnimation> Animations => animations;
    }
}
