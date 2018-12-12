﻿//Apache2, 2014-present, WinterDev

using System;
namespace LayoutFarm.UI
{
    public class NinespaceGrippers
    {
        NinespaceController _ninespaceController;
        public NinespaceGrippers(NinespaceController ninespaceController)
        {
            _ninespaceController = ninespaceController;
        }
        public AbstractRectUI LeftGripper { get; set; }
        public AbstractRectUI TopGripper { get; set; }
        public AbstractRectUI RightGripper { get; set; }
        public AbstractRectUI BottomGripper { get; set; }


        internal void UpdateGripperPositions()
        {
            switch (_ninespaceController.SpaceConcept)
            {
                default:
                    throw new NotSupportedException();
                case SpaceConcept.NineSpaceFree:
                    return;
                case SpaceConcept.NineSpace:
                case SpaceConcept.FiveSpace:
                    {
                        if (this.LeftGripper != null)
                        {
                            //align on center
                            this.LeftGripper.SetLocation(
                               _ninespaceController.LeftSpace.Width - (this.LeftGripper.Width / 2),
                              _ninespaceController.Owner.Height / 2);
                        }
                        if (this.RightGripper != null)
                        {
                            this.RightGripper.SetLocation(
                                _ninespaceController.RightSpace.X - (this.RightGripper.Width / 2),
                                _ninespaceController.Owner.Height / 2);
                        }

                        if (this.TopGripper != null)
                        {
                            this.TopGripper.SetLocation(
                                _ninespaceController.TopSpace.X + (_ninespaceController.TopSpace.Width / 2) - (this.TopGripper.Width / 2),
                                _ninespaceController.TopSpace.Bottom - (this.TopGripper.Height / 2));
                        }

                        if (this.BottomGripper != null)
                        {
                            this.BottomGripper.SetLocation(
                               _ninespaceController.BottomSpace.X + (_ninespaceController.BottomSpace.Width / 2) - (this.TopGripper.Width / 2),
                                _ninespaceController.BottomSpace.Y - (this.BottomGripper.Height / 2));
                        }
                    }
                    break;
                case SpaceConcept.FourSpace:
                    {
                    }
                    break;
                case SpaceConcept.ThreeSpaceHorizontal:
                    {
                    }
                    break;
                case SpaceConcept.ThreeSpaceVertical:
                    {
                    }
                    break;
                //------------------------------------
                case SpaceConcept.TwoSpaceHorizontal:
                    {
                    }
                    break;
                case SpaceConcept.TwoSpaceVertical:
                    {
                    }
                    break;
            }
        }
        internal void UpdateNinespaces()
        {
            switch (_ninespaceController.SpaceConcept)
            {
                default:
                    throw new NotSupportedException();
                case SpaceConcept.NineSpaceFree:
                    return;
                case SpaceConcept.NineSpace:
                case SpaceConcept.FiveSpace:
                    {
                        if (this.LeftGripper != null)
                        {
                            //align on center
                            _ninespaceController.SetLeftSpaceWidth(this.LeftGripper.Left + (this.LeftGripper.Width / 2));
                        }
                        if (this.RightGripper != null)
                        {
                            _ninespaceController.SetRightSpaceWidth(
                                 (_ninespaceController.Owner.Width - this.RightGripper.Left) - (this.RightGripper.Width / 2));
                        }
                        if (this.TopGripper != null)
                        {
                            _ninespaceController.SetTopSpaceHeight(this.TopGripper.Top + this.TopGripper.Height / 2);
                        }
                        if (this.BottomGripper != null)
                        {
                            _ninespaceController.SetBottomSpaceHeight(
                               (_ninespaceController.Owner.Height - this.BottomGripper.Top) - this.BottomGripper.Height / 2);
                        }
                    }
                    break;
                case SpaceConcept.FourSpace:
                    {
                    }
                    break;
                case SpaceConcept.ThreeSpaceHorizontal:
                    {
                    }
                    break;
                case SpaceConcept.ThreeSpaceVertical:
                    {
                    }
                    break;
                //------------------------------------
                case SpaceConcept.TwoSpaceHorizontal:
                    {
                    }
                    break;
                case SpaceConcept.TwoSpaceVertical:
                    {
                    }
                    break;
            }
        }
    }
}