namespace Atrasis.Laser.Battle.Logic
{
    using Atrasis.Laser.Battle.DataStream;
    using Atrasis.Laser.Battle.Logic.Input;
    using System;
    using System.Collections.Generic;

    public struct BulletData
    {
        public int instanceid;
        public int X, Y;
        public int TicksFlying;
        public int Angle;
    }

    public class LogicGameObjectManager
    {
        public int InputCounter = 0;

        public int PlayerX = 2550;
        public int PlayerY = 2550;

        public int BallX = 3150;
        public int BallY = 4950;

        public int BallHP = 1;

        public int DestinationX;
        public int DestinationY;

        public BulletData bullet;
        public int BallFlyTicks = 88888;
        public int BallAngle;
        public bool BallFlying => BallFlyTicks < 17;

        public LogicGameObjectManager()
        {
            DestinationX = PlayerX;
            DestinationY = PlayerY;

            bullet = new BulletData();
            bullet.TicksFlying = 899888;
        }

        public void HandleInput(List<ClientInput> inputs)
        {
            foreach (ClientInput input in inputs)
            {
                InputCounter = input.Counter;
                Console.WriteLine(input.Type);
                if (input.Type == 2)
                {
                    DestinationX = input.X;
                    DestinationY = input.Y;
                }
                else if (input.Type == 0)
                {
                    if (!BallAttached) Shoot(input.X, input.Y);
                    else ThrowBall(input.X, input.Y);
                }
            }
        }

        public void ThrowBall(int x, int y)
        {
            BallAttached = false;
            BallFlyTicks = 0;
            BallAngle = LogicMath.GetAngle(x, y);
        }

        public void Shoot(int x, int y)
        {
            if (bullet.TicksFlying >= 25)
            {
                int angle = LogicMath.GetAngle(x, y);

                bullet = new BulletData();
                bullet.X = PlayerX;
                bullet.Y = PlayerY;
                bullet.Angle = angle;
                bullet.TicksFlying = 0;
            }
        }

        internal const float angleCorrection = (float)(System.Math.PI * 90 / 180.0);

        public void UpdateBullet()
        {
            bullet.TicksFlying++;

            float speed = 0.22f;

            int VelocityX = 0;
            int VelocityY = 0;

            if (VelocityX == 0 && VelocityY == 0)
            {
                VelocityX += (int)(speed * (float)LogicMath.Cos((int)(bullet.Angle - angleCorrection)));
                VelocityY += (int)(speed * (float)LogicMath.Sin((int)(bullet.Angle - angleCorrection)));
            }

            bullet.X += VelocityX;
            bullet.Y += VelocityY;
        }

        public void Update()
        {
            if (bullet.TicksFlying < 25)
            {
                UpdateBullet();
            }

            if (!BallAttached)
            {
                if (PlayerX >= BallX - 150 && PlayerX <= BallX + 150 && PlayerY >= BallY - 150 && PlayerX <= BallY + 150 && !BallFlying)
                {
                    BallAttached = true;
                }
            }
            else
            {
                BallX = PlayerX;
                BallY = PlayerY;
            }

            if (BallFlying)
            {
                BallFlyTicks++;

                float speed = 0.1f;

                int VelocityX = 0;
                int VelocityY = 0;

                if (VelocityX == 0 && VelocityY == 0)
                {
                    VelocityX += (int)(speed * (float)LogicMath.Cos((int)(bullet.Angle - angleCorrection)));
                    VelocityY += (int)(speed * (float)LogicMath.Sin((int)(bullet.Angle - angleCorrection)));
                }

                BallX += VelocityX;
                BallY += VelocityY;
            }

            int deltaX = 0;
            int deltaY = 0;

            int movespeed = 45;

            if (DestinationX - PlayerX != 0)
            {
                bool block = false;

                if (DestinationX - PlayerX > 0) deltaX += LogicMath.Min(movespeed, DestinationX - PlayerX);
                else deltaX += LogicMath.Max(-movespeed, DestinationX - PlayerX);

                if (!block)
                {
                    PlayerX += deltaX;
                }
            }
            if (DestinationY - PlayerY != 0)
            {
                bool block = false;

                if (DestinationY - PlayerY > 0) deltaY += LogicMath.Min(movespeed, DestinationY - PlayerY);
                else deltaY += LogicMath.Max(-movespeed, DestinationY - PlayerY);
                if (!block)
                {
                    PlayerY += deltaY;
                }
            }
        }

        bool BallAttached = false;

        public void Encode(BitStream stream)
        {
            stream.WritePositiveInt(1000000, 21);
            stream.WritePositiveInt(0, 1);
            stream.WriteInt(-1, 4);

            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(1, 1);

            stream.WritePositiveInt(0, 5);
            stream.WritePositiveInt(0, 6);
            stream.WritePositiveInt(0, 5);
            stream.WritePositiveInt(0, 6);

            stream.WritePositiveInt(0, 1);
            stream.WritePositiveVInt(0, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(4000, 12);

            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);

            stream.WritePositiveVInt(0, 3); // rotate left

            stream.WritePositiveInt(0, 1); // rotate right
            stream.WritePositiveInt(1, 1); // !winner
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);

            stream.WritePositiveInt(0, 4);
            //stream.WritePositiveInt(1, 7);

            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);

            /*stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(3, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(3, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(3, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(3, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(3, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(1, 1);
            stream.WriteInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);*/

            bool isbullet = bullet.TicksFlying < 25;

            stream.WritePositiveInt(isbullet ? 3 : 2, 7); // GameObjects

            stream.WritePositiveInt(16, 5);
            stream.WritePositiveInt(3, 8);

            stream.WritePositiveInt(16, 5);
            stream.WritePositiveInt(47, 8);

            if (isbullet)
            {
                stream.WritePositiveInt(6, 5);
                stream.WritePositiveInt(15, 8);
            }

            /*stream.WritePositiveInt(16, 5);
            stream.WritePositiveInt(14, 8);
            stream.WritePositiveInt(16, 5);
            stream.WritePositiveInt(23, 8);
            stream.WritePositiveInt(16, 5);
            stream.WritePositiveInt(47, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);
            stream.WritePositiveInt(6, 5);
            stream.WritePositiveInt(0, 8);*/
            stream.WritePositiveInt(0, 14);
            stream.WritePositiveInt(1, 14);
            if (isbullet) stream.WritePositiveInt(0, 14);
            /*stream.WritePositiveInt(4, 14);
            stream.WritePositiveInt(5, 14);
            stream.WritePositiveInt(6, 14);
            stream.WritePositiveInt(0, 14);
            stream.WritePositiveInt(1, 14);
            stream.WritePositiveInt(2, 14);
            stream.WritePositiveInt(3, 14);
            stream.WritePositiveInt(4, 14);
            stream.WritePositiveInt(5, 14);
            stream.WritePositiveInt(6, 14);
            stream.WritePositiveInt(7, 14);
            stream.WritePositiveInt(8, 14);
            stream.WritePositiveInt(9, 14);*/

            packCharacter(stream, PlayerX, PlayerY, 3000);
            packBall(stream);
            if (isbullet) packProjectile(stream);
        }

        private void packBall(BitStream stream)
        {
            stream.WritePositiveVInt(BallX, 4);
            stream.WritePositiveVInt(BallY, 4);
            stream.WritePositiveVInt(102, 3);
            stream.WritePositiveVInt(0, 4);
            stream.WritePositiveInt(10, 4);
            stream.WritePositiveInt(0, 3);
            stream.WritePositiveInt(BallHP, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(1, 13);
            stream.WritePositiveInt(1, 13);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 9);
            stream.WritePositiveInt(0, 5);
        }

        private void packProjectile(BitStream stream)
        {
            stream.WritePositiveVInt(bullet.X, 4);
            stream.WritePositiveVInt(bullet.Y, 4);
            stream.WritePositiveVInt(0, 3);
            stream.WritePositiveVInt(450, 4);
            stream.WritePositiveInt(0, 3);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(284, 10);
            stream.WritePositiveInt(bullet.Angle, 9);
            stream.WritePositiveInt(0, 1);
        }

        private void packCharacter(BitStream stream, int x, int y, int hp)
        {
            stream.WritePositiveVInt(x, 4);
            stream.WritePositiveVInt(y, 4);
            stream.WritePositiveVInt(0, 3);
            stream.WritePositiveVInt(0, 4);
            stream.WritePositiveInt(10, 4);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 3);
            stream.WritePositiveInt(0, 1);
            stream.WriteInt(63, 6);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(1, 1);

            stream.WritePositiveInt(1, 1);

            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(hp, 13);
            stream.WritePositiveInt(3000, 13);

            stream.WritePositiveInt(1, 1);

            if (BallAttached)
            {
                stream.WritePositiveInt(1, 1); // has ball
                {
                    stream.WritePositiveVIntMax65535OftenZero(1);
                    stream.WritePositiveVIntMax65535OftenZero(1);
                }
            }
            else
            {
                stream.WritePositiveInt(0, 1);
            }

            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 4);
            stream.WritePositiveInt(0, 2);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 9);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(0, 5);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(3000, 12);
            stream.WritePositiveInt(1, 1);
            stream.WritePositiveInt(0, 1);
            stream.WritePositiveInt(1, 1);
        }
    }
}
