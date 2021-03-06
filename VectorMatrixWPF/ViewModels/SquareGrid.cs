﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using VectorMatrixClassLibrary;
using VectorMatrixWPF.Models;

namespace VectorMatrixWPF.ViewModels
{
    public class SquareGrid : INotifyPropertyChanged
    {
        //////////////////
        // CONSTRUCTORS //
        //////////////////

        public SquareGrid() { }
        public SquareGrid(int maxGridSize) { MaxSize = maxGridSize; }

        //////////////////////////
        // MEMBERS & PROPERTIES //
        //////////////////////////

        // MODEL INSTANCES
        public static Animation Animation { get; } = new Animation(true);

        // MATHMATICAL PROPERTIES
        public int MaxSize { get; set; } = 10;

        private DWVector IHat { get; set; } = new DWVector();
        private DWVector JHat { get; set; } = new DWVector();

        // LINE COLLECTIONS 
        private ObservableCollection<DWLine> _basisVectors = new ObservableCollection<DWLine>();
        private ObservableCollection<DWLine> _vectorLines = new ObservableCollection<DWLine>();
        private ObservableCollection<DWGridLine> _dynamicGridLines = new ObservableCollection<DWGridLine>();

        public ObservableCollection<DWLine> BasisVectors => _basisVectors;
        public ObservableCollection<DWLine> VectorLines => _vectorLines;
        public ObservableCollection<DWGridLine> DynamicGridLines => _dynamicGridLines;
        public ObservableCollection<DWLine> StaticGridLines { get; } = new ObservableCollection<DWLine>();

        // CANVAS PROPERTIES
        public double UnitLength    { get; set; }
        public double CanvasHeight  { get; set; }
        public double CanvasWidth   { get; set; }
        public double CanvasXOrigin { get; set; }
        public double CanvasYOrigin { get; set; }

        /////////////
        // METHODS //
        /////////////

        // PROPERTY CHANGED IMPLEMENTATION
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // CANVAS METHODS

        public void ChangeAnimationFactor(string factor) =>
            Animation.AnimationFactor = Animation.Speeds[factor];

        /// <summary>
        /// Sets the canvas properties based on the height/width of the plane being used.
        /// Also sets i-hat and j-hat to regular X Y coords
        /// </summary>
        /// <param name="element"></param>
        /// <param name="e"></param>
        public void InitialiseCanvasElement(object element, RoutedEventArgs e) =>
            InitialiseCanvasElement(element);

        public void InitialiseCanvasElement(object element)
        {
            if (element is Canvas plane)
            {
                CanvasHeight = plane.ActualHeight;
                CanvasWidth = plane.ActualWidth;
                CanvasXOrigin = CanvasWidth / 2.0;
                CanvasYOrigin = CanvasHeight / 2.0;
                UnitLength = CanvasWidth / (MaxSize * 2.0);
                IHat.X = 1;
                IHat.Y = 0;
                JHat.X = 0;
                JHat.Y = 1;
            }
        }

        /// <summary>
        /// Sets whether the gridlines are shown (active) or not.
        /// Can set the state of both the static grid or the dynamic grid using the params
        /// </summary>
        /// <param name="activate">Bool: Whether the gridlines should be active or not</param>
        /// <param name="isStaticGrid">Bool: Whether the grid to be targeted is the static grid or not</param>
        public void ChangeGridLineState(bool activate, bool isStaticGrid)
        {
            if (isStaticGrid)
            {
                if (StaticGridLines.Count > 0)
                {
                    foreach (DWLine gridline in StaticGridLines)
                    {
                        if (activate) gridline.IsActive = true;
                        else gridline.IsActive = false;
                    }
                }
                else
                    AddStaticGridLines(activate);
            }
            else
            {
                if (DynamicGridLines.Count > 0)
                {
                    foreach (DWGridLine gridline in DynamicGridLines)
                    {
                        if (activate) gridline.DWLine.IsActive = true;
                        else gridline.DWLine.IsActive = false;
                    }
                }
                else
                    AddDynamicGridLines(activate);
            }
        }

        /// <summary>
        /// Adds the dynamic gridlines to GridLines list.
        /// They are created active as default, but the optional param can set this to false
        /// </summary>
        /// <param name="isactive"></param>
        public void AddDynamicGridLines(bool isactive = true)
        {
            SolidColorBrush color = Brushes.Blue;

            for (int i = 0; i <= MaxSize * 2; i++)
            {
                DWGridLine xline = new DWGridLine(new DWVector(MaxSize - i, MaxSize), new DWVector(MaxSize - i, -MaxSize),
                    new DWLine
                    (
                        0, 0, LineType.GRID,
                        new Line
                        {
                            Stroke = color,
                            StrokeThickness = 0.5,
                            X1 = UnitLength * i,
                            Y1 = 0,
                            X2 = UnitLength * i,
                            Y2 = CanvasHeight
                        },
                        color,
                        isactive
                    ));

                DWGridLine yline = new DWGridLine(new DWVector(-MaxSize, MaxSize - i), new DWVector(MaxSize, MaxSize - i),
                    new DWLine
                    (
                        0, 0, LineType.GRID,
                        new Line
                        {
                            Stroke = color,
                            StrokeThickness = 0.5,
                            X1 = 0,
                            Y1 = UnitLength * i,
                            X2 = CanvasWidth,
                            Y2 = UnitLength * i
                        },
                        color,
                        isactive
                    ));

                DynamicGridLines.Add(xline);
                DynamicGridLines.Add(yline);
            }
        }

        /// <summary>
        /// Adds the static gridlines to VectorLines list.
        /// They are created active as default, but the optional param can set this to false
        /// </summary>
        /// <param name="isactive"></param>
        public void AddStaticGridLines(bool isactive = true)
        {
            SolidColorBrush color = Brushes.Gray;
            for (int i = 0; i <= MaxSize * 2; i++)
            {
                DWLine xline = new DWLine
                (
                    i, 1, LineType.GRIDORIG,
                    new Line
                    {
                        Stroke = color,
                        StrokeThickness = 0.3,
                        X1 = UnitLength * i,
                        Y1 = 0,
                        X2 = UnitLength * i,
                        Y2 = CanvasHeight
                    },
                    color,
                    isactive
                );

                DWLine yline = new DWLine
                (
                    1, i, LineType.GRIDORIG,
                    new Line
                    {
                        Stroke = color,
                        StrokeThickness = 0.3,
                        X1 = 0,
                        Y1 = UnitLength * i,
                        X2 = CanvasWidth,
                        Y2 = UnitLength * i
                    },
                    color,
                    isactive
                );

                StaticGridLines.Add(xline);
                StaticGridLines.Add(yline);
            }
        }

        // VECTOR METHODS

        /// <summary>
        /// Adds or removes active or inactive lines, respectively, to/from the canvas.
        /// Loops through both DWLines and DWGridLines
        /// </summary>
        /// <param name="plane"></param>
        public void ShowActiveLines(Canvas plane)
        {
            List<DWLine> dwlines = new List<DWLine>();
            dwlines.AddRange(VectorLines);
            dwlines.AddRange(StaticGridLines);
            dwlines.AddRange(BasisVectors);
            dwlines.AddRange(DynamicGridLines.Select(x => x.DWLine));

            foreach (DWLine dwline in dwlines)
            {
                if (dwline.IsActive)
                {
                    if (!plane.Children.Contains(dwline.Line))
                        plane.Children.Add(dwline.Line);
                }
                else
                    plane.Children.Remove(dwline.Line);
            }
        }

        /// <summary>
        /// Add two basis vector lines (1 i-hat and 1 j-hat) to the VetorLines list
        /// </summary>
        public void AddBasisVectors()
        {
            BasisVectors.Add(new DWLine
            (
                IHat.X, IHat.Y,
                LineType.BASE,
                new Line
                {
                    Stroke = Brushes.LightGreen,
                    StrokeThickness = 3,
                    X1 = CanvasXOrigin,
                    Y1 = CanvasYOrigin,
                    X2 = CanvasXOrigin + (IHat.X * UnitLength),
                    Y2 = CanvasYOrigin - (IHat.Y * UnitLength)
                }, Brushes.LightGreen
            ));

            BasisVectors.Add(new DWLine
            (
                JHat.X, JHat.Y,
                LineType.BASE,
                new Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 3,
                    X1 = CanvasXOrigin,
                    Y1 = CanvasYOrigin,
                    X2 = CanvasXOrigin + (JHat.X * UnitLength),
                    Y2 = CanvasYOrigin - (JHat.Y * UnitLength)
                }, Brushes.Red
            ));
        }

        /// <summary>
        /// Sets all the basis vectors to active, or creates them if they don't exist
        /// </summary>
        public void ActivateBasisVectors(bool active)
        {
            if (BasisVectors.Count == 0)
                AddBasisVectors();

            if (active)
            {
                foreach (DWLine dWLine in BasisVectors)
                {
                    dWLine.IsActive = true;
                }
            }
            else
            {
                foreach (DWLine dwline in BasisVectors)
                {
                    dwline.IsActive = false;
                }
            }
        }

        /// <summary>
        /// Add a Vector to the VectorLines list.
        /// Each vector has a tooltip and MouseEnter/MouseLeave events
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddVector(double x, double y)
        {
            DWVector vector = VectorMath.GetNewVectorLocation(IHat, JHat, x, y);
            SolidColorBrush color = PickRandomColor();

            DWLine vectorLine = new DWLine
            (
                x, y, LineType.VECTOR,
                new Line
                {
                    Stroke = color,
                    X1 = CanvasXOrigin,
                    Y1 = CanvasYOrigin,
                    X2 = CanvasXOrigin + (vector.X * UnitLength),
                    Y2 = CanvasYOrigin - (vector.Y * UnitLength)
                }, color
            );

            vectorLine.Line.ToolTip = $"X: {vectorLine.X}\nY: {vectorLine.Y}";

            vectorLine.Line.MouseEnter += MouseEnterVectorHighlight;
            vectorLine.Line.MouseLeave += MouseLeaveVectorHighlight;

            VectorLines.Add(vectorLine);
        }

        /// <summary>
        /// Animates the rotation based on the animation speed
        /// </summary>
        /// <param name="degree"></param>
        public void AnimateRotation(double degree, bool undo = false)
        {
            DWMatrix currentMatrix = new DWMatrix(IHat.X, IHat.Y, JHat.X, JHat.Y);
            DWMatrix targetMatrix = VectorMath.RotateNRadiansAntiClockwise(new DWMatrix(IHat, JHat), ((degree * Math.PI) / 180));

            // jump to full rotation if animation is off
            if (!Animation.AnimationEnabled)
            {
                RotateNDegreesAntiClockwise(degree);
            }
            else
            {
                // rotates a part of the way and then re-renders
                Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < Animation.GetFactoredSpeed(); i++)
                    {
                        RotateNDegreesAntiClockwise(degree / (Animation.GetFactoredSpeed()));
                        Application.Current.Dispatcher.Invoke(delegate { }, System.Windows.Threading.DispatcherPriority.Render);
                        Thread.Sleep(1);
                    }
                }));

            }

            if (!undo) Animation.TransformationsList.Add(
                new Transformation(currentMatrix, targetMatrix, VectorMath.GetInverseMatrix(targetMatrix), degree)
            );
        }

        /// <summary>
        /// Calculates a new plane after a rotation (based on VectorMath), then sets the values of i-hat and j-hat to the new plane.
        /// Uses radian conversion to work in a 360 space
        /// </summary>
        /// <param name="degrees"></param>
        public void RotateNDegreesAntiClockwise(double degrees)
        {
            DWMatrix newPlane = VectorMath.RotateNRadiansAntiClockwise(new DWMatrix(IHat, JHat), ((degrees * Math.PI) / 180));

            IHat.X = newPlane.IX;
            IHat.Y = newPlane.IY;
            JHat.X = newPlane.JX;
            JHat.Y = newPlane.JY;

            UpdateVectorLines();
        }

        /// <summary>
        /// Calls the RotateNDegreesAntiClockwise with a values of -90
        /// </summary>
        public void Rotate90DegreesClockwise() => AnimateRotation(-90);

        /// <summary>
        /// Calls the RotateNDegreesAntiClockwise with a values of 90
        /// </summary>
        public void Rotate90DegreesAntiClockwise() => AnimateRotation(90);

        // TRANSFORMATION METHODS

        /// <summary>
        /// Animates the linear transformation if animations enabled
        /// </summary>
        /// <param name="currentMatrix"></param>
        /// <param name="targetMatrix"></param>
        /// <param name="stepMatrix"></param>
        public void AnimateTransformation(DWMatrix currentMatrix, DWMatrix targetMatrix)
        {
            DWMatrix transformedTarget = VectorMath.MakeLinearTransformation(currentMatrix, targetMatrix);

            Animation.TransformationsList.Add(
                new Transformation(currentMatrix, transformedTarget, VectorMath.GetInverseMatrix(targetMatrix), 0)
            );

            if (!Animation.AnimationEnabled) TransformPlane(transformedTarget);

            else
            {
                DWMatrix stepMatrix = (transformedTarget - currentMatrix) / (Animation.GetFactoredSpeed());
                Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < (Animation.GetFactoredSpeed()); i++)
                    {
                        DWMatrix newPlane = currentMatrix + stepMatrix;

                        IHat.X = newPlane.IX;
                        IHat.Y = newPlane.IY;
                        JHat.X = newPlane.JX;
                        JHat.Y = newPlane.JY;
                        UpdateVectorLines();

                        Application.Current.Dispatcher.Invoke(delegate { }, System.Windows.Threading.DispatcherPriority.Render);
                        Thread.Sleep(1);
                        currentMatrix = newPlane;
                    }
                }));
            }
        }
        
        public void AnimateTransformation(double ix, double iy, double jx, double jy) =>
            AnimateTransformation(
                new DWMatrix(IHat.X, IHat.Y, JHat.X, JHat.Y),
                new DWMatrix(ix, iy, jx, jy)
            );

        /// <summary>
        /// Animates the inversion of a transformation
        /// </summary>
        /// <param name="initMatrix"></param>
        /// <param name="transformedMatrix"></param>
        /// <param name="inverseMatrix"></param>
        public void AnimateInversion(DWMatrix initMatrix, DWMatrix transformedMatrix)
        {

            if (!Animation.AnimationEnabled)
            {
                IHat.X = initMatrix.IX;
                IHat.Y = initMatrix.IY;
                JHat.X = initMatrix.JX;
                JHat.Y = initMatrix.JY;
                UpdateVectorLines();
            }

            else
            {
                DWMatrix stepMatrix = (initMatrix - transformedMatrix) / (Animation.GetFactoredSpeed());
                Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < (Animation.GetFactoredSpeed()); i++)
                    {
                        DWMatrix newPlane = transformedMatrix + stepMatrix;

                        IHat.X = newPlane.IX;
                        IHat.Y = newPlane.IY;
                        JHat.X = newPlane.JX;
                        JHat.Y = newPlane.JY;
                        UpdateVectorLines();

                        Application.Current.Dispatcher.Invoke(delegate { }, System.Windows.Threading.DispatcherPriority.Render);
                        Thread.Sleep(1);
                        transformedMatrix = newPlane;
                    }
                }));
            }

        }

        /// <summary>
        /// Takes a matrix to perform a linear transformation.
        /// Sets the values of i-hat and j-hat based on this transformation matrix (matrix multiplication)
        /// </summary>
        /// <param name="matrix"></param>
        public void TransformPlane(DWMatrix matrix)
        {
            DWMatrix newPlane = VectorMath.MakeLinearTransformation(matrix, new DWMatrix(IHat, JHat));

            IHat.X = newPlane.IX;
            IHat.Y = newPlane.IY;
            JHat.X = newPlane.JX;
            JHat.Y = newPlane.JY;
            UpdateVectorLines();
        }

        /// <summary>
        /// Shears the plane (sets x of j-hat to 1)
        /// </summary>
        public void ShearPlane() => AnimateTransformation(new DWMatrix(IHat.X, IHat.Y, JHat.X, JHat.Y), new DWMatrix(1, 0, 1, 1));

        /// <summary>
        /// Reset the i-hat and j-hat values back to their default state and informs all vectors
        /// </summary>
        public void UndoLastTransformation()
        {
            if (Animation.TransformationsList.Count != 0)
            {

                var lastItem = Animation.TransformationsList.Last();

                if (lastItem.Degrees == 0)
                    AnimateInversion(lastItem.InitialPlane, lastItem.NewPlane);
                else
                    AnimateRotation(-lastItem.Degrees, true);

                Animation.TransformationsList.Remove(Animation.TransformationsList.Last());

            }
        }

        // PRIVATE HELPER METHODS

        /// <summary>
        /// Picks a random colour from a set collection of 5 different colours
        /// </summary>
        /// <returns>Returns a SoldColorBrush colour from the set list</returns>
        private SolidColorBrush PickRandomColor()
        {
            List<SolidColorBrush> colors = new List<SolidColorBrush>() {
                Brushes.Red,
                Brushes.DarkRed,
                Brushes.LawnGreen,
                Brushes.DarkBlue,
                Brushes.Purple,
                Brushes.Gold,
                Brushes.DarkOrange
            };

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int choice = rand.Next(colors.Count);

            return colors[choice];
        }

        /// <summary>
        /// Gets the current vector location of each dynamic gridline line (start and end) and each vector.
        /// This is based on the current i-hat and j-hat values.
        /// Used to get correct vectors after linear transformations
        /// </summary>
        private void UpdateVectorLines() =>        
            Animation.UpdateLines(ref _vectorLines, ref _basisVectors, ref _dynamicGridLines, IHat, JHat, CanvasXOrigin, CanvasYOrigin, UnitLength);

        // VECTOR LINE MOUSE EVENTS

        /// <summary>
        /// Thickens the vector line if the mouse is over it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseEnterVectorHighlight(object sender, RoutedEventArgs e)
        {
            if (sender is Line line)
                line.StrokeThickness = 3;
        }

        /// <summary>
        /// Returns vector line thickness to normal when mouse is not over it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeaveVectorHighlight(object sender, RoutedEventArgs e)
        {
            if (sender is Line line)
                line.StrokeThickness = 1;
        }

    }
}
