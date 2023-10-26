using DiligentEngine.RT.Sprites;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Players
{
    public abstract class PlayerSprite : IPlayerSprite
    {
        const float SpriteWidth = 128f;
        const float SpriteHeight = 70f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        const int spriteWalkFrameSpeed = (int)(0.2f * Clock.SecondsToMicro);
        const int victoryFrameSpeed = (int)(0.31f * Clock.SecondsToMicro);

        public SpriteMaterialDescription Tier1 { get; protected set; }

        public SpriteMaterialDescription Tier2 { get; protected set; }

        public SpriteMaterialDescription Tier3 { get; protected set; }

        public Dictionary<String, SpriteAnimation> Animations => animations;

        /*********************************************
         * 
         * To make a sprite for this animation add the textures in the following order to the image atlas packer
         * 
         * Back right dominant
         * Back left dominant
         * Front left dominant
         * Front right dominant
         * Side wide
         * Side narrow
         * Front stand
         * Back stand
         * 
         * 128 x 64
         * 
         ********************************************/

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "stand-down", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 2, SpriteStepY * 1, SpriteStepX * 3, SpriteStepY * 2)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 21, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(29, 21, -0.01f, 32, 32), //Left Hand
                    }
                } )
            },
            { "stand-left", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 4, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16, 23, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(16, 23, -0.01f, 32, 32), //Left Hand
                    }
                })
            },
            { "stand-right", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 4, SpriteStepY * 0, SpriteStepX * 3, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16, 23, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(16, 23, +0.01f, 32, 32), //Left Hand
                    }
                } )
            },
            { "stand-up", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 1, SpriteStepX * 4, SpriteStepY * 2)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(29, 21, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(3, 21, +0.01f, 32, 32), //Left Hand
                    }
                } )
            },
            { "down", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 1, SpriteStepY * 0, SpriteStepX * 2, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 22, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(29, 20, -0.01f, 32, 32), //Left Hand
                    }
                },
                new SpriteFrame(SpriteStepX * 1, SpriteStepY * 1, SpriteStepX * 2, SpriteStepY * 2)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 20, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(29, 22, -0.01f, 32, 32), //Left Hand
                    }
                } )
            },
            { "up", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 0, SpriteStepY * 0, SpriteStepX * 1, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(29, 20, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(3, 22, +0.01f, 32, 32), //Left Hand
                    }
                },
                new SpriteFrame(SpriteStepX * 0, SpriteStepY * 1, SpriteStepX * 1, SpriteStepY * 2)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(29, 22, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(3, 20, +0.01f, 32, 32), //Left Hand
                    }
                } )
            },
            { "right", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 2, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 24, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(23, 21, +0.01f, 32, 32), //Left Hand
                    }
                },
                new SpriteFrame(SpriteStepX * 4, SpriteStepY * 0, SpriteStepX * 3, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16, 23, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(16, 23, +0.01f, 32, 32), //Left Hand
                    }
                })
            },
            { "left", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 2, SpriteStepY * 0, SpriteStepX * 3, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 21, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(20, 24, -0.01f, 32, 32), //Left Hand
                    }
                },
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 4, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16, 23, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(16, 23, -0.01f, 32, 32), //Left Hand
                    }
                })
            },
            { "victory", new SpriteAnimation(victoryFrameSpeed,
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 4, SpriteStepY * 1)
                //new SpriteFrame(SpriteStepX * 2, SpriteStepY * 0, SpriteStepX * 3, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 21, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(20, 24, -0.01f, 32, 32), //Left Hand
                    }
                },
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 4, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16, 23, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(16, 23, -0.01f, 32, 32), //Left Hand
                    }
                })
            },
            { "cast-left", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 2, SpriteStepY * 0, SpriteStepX * 3, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 21, +0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(20, 24, -0.01f, 32, 32), //Left Hand
                    }
                })
            },
            { "cast-right", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(SpriteStepX * 3, SpriteStepY * 0, SpriteStepX * 2, SpriteStepY * 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 24, -0.01f, 32, 32), //Right Hand
                        SpriteFrameAttachment.FromFramePosition(23, 21, +0.01f, 32, 32), //Left Hand
                    }
                })
            },
            
            //Right Hand
            { "stand-down-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-left-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-right-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-up-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "down-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "up-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "right-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "left-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "victory-r-hand", new SpriteAnimation(victoryFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "cast-left-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "cast-right-r-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(0 / SpriteWidth, 64 / SpriteHeight, 6 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            
            //Left Hand
            { "stand-down-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-left-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-right-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "stand-up-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "down-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "up-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "right-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "left-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "victory-l-hand", new SpriteAnimation(victoryFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "cast-left-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, 0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
            { "cast-right-l-hand", new SpriteAnimation(spriteWalkFrameSpeed,
                new SpriteFrame(6 / SpriteWidth, 64 / SpriteHeight, 12 / SpriteWidth, 70 / SpriteHeight)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(3, 2, -0.02f, 6, 6), //Center of hand, aligns to above hand centers
                    }
                } )
            },
        };
    }
}
