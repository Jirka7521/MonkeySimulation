﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonkeySimulation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private double monkeyHeight = 5; // Default value in meters
    private double shooterDistance = 10; // Default value in meters

    // Scaling factors for coordinate system
    private double xScale = 20; // pixels per meter
    private double yScale = 20; // pixels per meter
    private double xMargin = 60;
    private double yMargin = 40;

    public MainWindow()
    {
        InitializeComponent();
        DrawScene();
    }

    private void DrawScene()
    {
        // Clear the canvas before redrawing
        simulationCanvas.Children.Clear();

        double canvasWidth = simulationCanvas.ActualWidth;
        double canvasHeight = simulationCanvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
            return; // Not ready to draw yet

        // Calculate available drawing space
        double availableWidth = canvasWidth - xMargin * 1.5;
        double availableHeight = canvasHeight - yMargin * 1.5;

        // Check if we need to adjust the scale to fit the scene
        bool adjustScale = false;

        // If scene is too big for current scale - halve the scale
        if (shooterDistance * xScale > availableWidth || monkeyHeight * yScale > availableHeight)
        {
            // Determine which scale factor needs more adjustment
            double xRatio = (shooterDistance * xScale) / availableWidth;
            double yRatio = (monkeyHeight * yScale) / availableHeight;

            if (xRatio > yRatio && xRatio > 1.0)
            {
                // Need to halve x scale (repeatedly if needed)
                while (shooterDistance * xScale > availableWidth && xScale > 1.0)
                {
                    xScale /= 2;
                    adjustScale = true;
                }
            }

            if (yRatio > xRatio && yRatio > 1.0)
            {
                // Need to halve y scale (repeatedly if needed)
                while (monkeyHeight * yScale > availableHeight && yScale > 1.0)
                {
                    yScale /= 2;
                    adjustScale = true;
                }
            }
        }
        // If scene is too small (less than half of available space) - double the scale
        else if (shooterDistance * xScale < availableWidth * 0.4 && monkeyHeight * yScale < availableHeight * 0.4)
        {
            // Double the scale but keep x and y scales in sync
            xScale *= 2;
            yScale *= 2;
            adjustScale = true;
        }

        // Ensure minimum scale
        if (xScale < 1.0) xScale = 1.0;
        if (yScale < 1.0) yScale = 1.0;

        // If we adjusted the scale, log it
        if (adjustScale)
        {
            Console.WriteLine($"Adjusted scale to: X={xScale}, Y={yScale}");
        }

        // Calculate the maximum values for our coordinate system based on current scales
        double maxX = Math.Max(shooterDistance * 1.2, availableWidth / xScale);
        double maxY = Math.Max(monkeyHeight * 1.2, availableHeight / yScale);

        // Draw axes
        DrawAxes(canvasWidth, canvasHeight, maxX, maxY);

        // Draw the ground
        Line ground = new Line
        {
            X1 = xMargin,
            Y1 = canvasHeight - yMargin,
            X2 = xMargin + maxX * xScale,
            Y2 = canvasHeight - yMargin,
            Stroke = Brushes.Brown,
            StrokeThickness = 2
        };
        simulationCanvas.Children.Add(ground);

        // Draw the hunter (shooter)
        DrawShooter(xMargin, canvasHeight - yMargin);
        // Draw the monkey
        DrawMonkey(xMargin + shooterDistance * xScale, monkeyHeight, canvasHeight - yMargin);

    }

    private void DrawAxes(double canvasWidth, double canvasHeight, double maxX, double maxY)
    {
        // Y-axis
        Line yAxis = new Line
        {
            X1 = xMargin,
            Y1 = canvasHeight - yMargin,
            X2 = xMargin,
            Y2 = 10,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        simulationCanvas.Children.Add(yAxis);

        // X-axis
        Line xAxis = new Line
        {
            X1 = xMargin,
            Y1 = canvasHeight - yMargin,
            X2 = canvasWidth - 10,
            Y2 = canvasHeight - yMargin,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        simulationCanvas.Children.Add(xAxis);

        // Draw x-axis markers
        int xInterval = DetermineInterval(maxX);
        for (int x = 0; x <= (int)maxX; x += xInterval)
        {
            if (x == 0) continue; // Skip origin

            double xPos = xMargin + x * xScale;

            // Tick mark
            Line tick = new Line
            {
                X1 = xPos,
                Y1 = canvasHeight - yMargin,
                X2 = xPos,
                Y2 = canvasHeight - yMargin + 5,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            simulationCanvas.Children.Add(tick);

            // Label
            TextBlock label = new TextBlock
            {
                Text = x.ToString(),
                FontSize = 10
            };
            Canvas.SetLeft(label, xPos - 5);
            Canvas.SetTop(label, canvasHeight - yMargin + 7);
            simulationCanvas.Children.Add(label);
        }

        // Draw y-axis markers
        int yInterval = DetermineInterval(maxY);
        for (int y = 0; y <= (int)maxY; y += yInterval)
        {
            if (y == 0) continue; // Skip origin

            double yPos = canvasHeight - yMargin - y * yScale;

            // Tick mark
            Line tick = new Line
            {
                X1 = xMargin,
                Y1 = yPos,
                X2 = xMargin - 5,
                Y2 = yPos,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            simulationCanvas.Children.Add(tick);

            // Label
            TextBlock label = new TextBlock
            {
                Text = y.ToString(),
                FontSize = 10
            };
            Canvas.SetLeft(label, xMargin - 25);
            Canvas.SetTop(label, yPos - 7);
            simulationCanvas.Children.Add(label);
        }

        // Axis labels
        TextBlock xAxisLabel = new TextBlock
        {
            Text = "Distance (m)",
            FontSize = 12,
            FontWeight = FontWeights.Bold
        };
        Canvas.SetLeft(xAxisLabel, canvasWidth / 2);
        Canvas.SetTop(xAxisLabel, canvasHeight - 20);
        simulationCanvas.Children.Add(xAxisLabel);

        TextBlock yAxisLabel = new TextBlock
        {
            Text = "Height (m)",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            RenderTransform = new RotateTransform(-90)
        };
        Canvas.SetLeft(yAxisLabel, 10);
        Canvas.SetTop(yAxisLabel, canvasHeight / 2);
        simulationCanvas.Children.Add(yAxisLabel);
    }

    private int DetermineInterval(double maxValue)
    {
        if (maxValue <= 10) return 1;
        if (maxValue <= 20) return 2;
        if (maxValue <= 50) return 5;
        if (maxValue <= 100) return 10;
        return 20;
    }

    private void UpdateSimulation_Click(object sender, RoutedEventArgs e)
    {
        // Validate inputs
        if (double.TryParse(heightTextBox.Text, out double height) &&
            double.TryParse(distanceTextBox.Text, out double distance))
        {
            if (height >= 1 && distance >= 1)
            {
                monkeyHeight = height;
                shooterDistance = distance;
                DrawScene();
                errorTextBlock.Text = string.Empty;
            }
            else
            {
                errorTextBlock.Text = "Both height and distance must be at least 1.";
            }
        }
        else
        {
            errorTextBlock.Text = "Please enter valid numbers.";
        }
    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawScene();
    }
    /// <summary>
    /// Draw the shooter (hunter) on the canvas
    /// </summary>
    /// <param name="x">Where to start on X axis</param>
    /// <param name="groundY">Where to start on Y axis</param>
    private void DrawShooter(double x, double groundY)
    {
        double shooterHeight = Math.Min(80, Math.Max(30, yScale * 2));
        double headSize = shooterHeight * 0.25;
        double bodyWidth = shooterHeight * 0.4;

        // Head
        Ellipse head = new Ellipse
        {
            Width = headSize,
            Height = headSize,
            Fill = Brushes.Bisque,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(head, x - headSize / 2);
        Canvas.SetTop(head, groundY - shooterHeight);
        simulationCanvas.Children.Add(head);

        // Eyes
        double eyeSize = headSize * 0.2;
        Ellipse leftEye = new Ellipse
        {
            Width = eyeSize,
            Height = eyeSize,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(leftEye, x - headSize * 0.25);
        Canvas.SetTop(leftEye, groundY - shooterHeight + headSize * 0.3);
        simulationCanvas.Children.Add(leftEye);

        Ellipse rightEye = new Ellipse
        {
            Width = eyeSize,
            Height = eyeSize,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(rightEye, x + headSize * 0.05);
        Canvas.SetTop(rightEye, groundY - shooterHeight + headSize * 0.3);
        simulationCanvas.Children.Add(rightEye);

        // Pupils
        double pupilSize = eyeSize * 0.6;
        Ellipse leftPupil = new Ellipse
        {
            Width = pupilSize,
            Height = pupilSize,
            Fill = Brushes.Black
        };
        Canvas.SetLeft(leftPupil, x - headSize * 0.25 + (eyeSize - pupilSize) / 2);
        Canvas.SetTop(leftPupil, groundY - shooterHeight + headSize * 0.3 + (eyeSize - pupilSize) / 2);
        simulationCanvas.Children.Add(leftPupil);

        Ellipse rightPupil = new Ellipse
        {
            Width = pupilSize,
            Height = pupilSize,
            Fill = Brushes.Black
        };
        Canvas.SetLeft(rightPupil, x + headSize * 0.05 + (eyeSize - pupilSize) / 2);
        Canvas.SetTop(rightPupil, groundY - shooterHeight + headSize * 0.3 + (eyeSize - pupilSize) / 2);
        simulationCanvas.Children.Add(rightPupil);

        // Body
        Rectangle body = new Rectangle
        {
            Width = bodyWidth,
            Height = shooterHeight * 0.5,
            Fill = Brushes.DarkGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(body, x - bodyWidth / 2);
        Canvas.SetTop(body, groundY - shooterHeight + headSize);
        simulationCanvas.Children.Add(body);

        // Legs
        double legWidth = bodyWidth * 0.3;
        double legHeight = shooterHeight * 0.25;

        Rectangle leftLeg = new Rectangle
        {
            Width = legWidth,
            Height = legHeight,
            Fill = Brushes.DarkOliveGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(leftLeg, x - bodyWidth * 0.4);
        Canvas.SetTop(leftLeg, groundY - legHeight);
        simulationCanvas.Children.Add(leftLeg);

        Rectangle rightLeg = new Rectangle
        {
            Width = legWidth,
            Height = legHeight,
            Fill = Brushes.DarkOliveGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(rightLeg, x + bodyWidth * 0.1);
        Canvas.SetTop(rightLeg, groundY - legHeight);
        simulationCanvas.Children.Add(rightLeg);

        // Arms
        double armWidth = bodyWidth * 0.7;
        double armHeight = bodyWidth * 0.25;

        // Left arm
        Rectangle leftArm = new Rectangle
        {
            Width = armWidth * 0.6,
            Height = armHeight,
            Fill = Brushes.DarkGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(leftArm, x - bodyWidth * 0.5 - armWidth * 0.3);
        Canvas.SetTop(leftArm, groundY - shooterHeight + headSize + body.Height * 0.2);
        simulationCanvas.Children.Add(leftArm);

        // Right arm (holding gun)
        Rectangle rightArm = new Rectangle
        {
            Width = armWidth * 0.6,
            Height = armHeight,
            Fill = Brushes.DarkGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            RenderTransform = new RotateTransform(15)
        };
        Canvas.SetLeft(rightArm, x + bodyWidth * 0.3);
        Canvas.SetTop(rightArm, groundY - shooterHeight + headSize + body.Height * 0.2);
        simulationCanvas.Children.Add(rightArm);

        // Gun
        Rectangle gunBase = new Rectangle
        {
            Width = armWidth * 0.9,
            Height = armHeight * 0.6,
            Fill = Brushes.Black,
            Stroke = Brushes.DarkGray,
            StrokeThickness = 1,
            RenderTransform = new RotateTransform(15)
        };
        Canvas.SetLeft(gunBase, x + bodyWidth * 0.6);
        Canvas.SetTop(gunBase, groundY - shooterHeight + headSize + body.Height * 0.2);
        simulationCanvas.Children.Add(gunBase);

        Rectangle gunBarrel = new Rectangle
        {
            Width = armWidth * 1.2,
            Height = armHeight * 0.3,
            Fill = Brushes.DimGray,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5,
            RenderTransform = new RotateTransform(15)
        };
        Canvas.SetLeft(gunBarrel, x + bodyWidth * 0.95);
        Canvas.SetTop(gunBarrel, groundY - shooterHeight + headSize + body.Height * 0.25);
        simulationCanvas.Children.Add(gunBarrel);

        // Hat
        Path hat = new Path
        {
            Fill = Brushes.DarkOliveGreen,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry hatGeometry = new PathGeometry();
        PathFigure hatFigure = new PathFigure();

        hatFigure.StartPoint = new Point(x - headSize * 0.7, groundY - shooterHeight + headSize * 0.1);

        hatFigure.Segments.Add(new LineSegment(new Point(x + headSize * 0.7, groundY - shooterHeight + headSize * 0.1), true));
        hatFigure.Segments.Add(new LineSegment(new Point(x + headSize * 0.4, groundY - shooterHeight - headSize * 0.3), true));
        hatFigure.Segments.Add(new LineSegment(new Point(x - headSize * 0.4, groundY - shooterHeight - headSize * 0.3), true));
        hatFigure.Segments.Add(new LineSegment(new Point(x - headSize * 0.7, groundY - shooterHeight + headSize * 0.1), true));

        hatFigure.IsClosed = true;
        hatGeometry.Figures.Add(hatFigure);
        hat.Data = hatGeometry;

        simulationCanvas.Children.Add(hat);
    }

    private void DrawMonkey(double x, double treeHeight, double groundY)
    {
        double monkeyScale = Math.Min(30, Math.Max(15, Math.Min(xScale, yScale) * 0.75));

        // Draw tree first so it appears behind the monkey
        DrawTree(x, treeHeight, groundY);

        // Get branch parameters
        double branchHeight = treeHeight * 0.85; // Position branch higher on trunk
        double branchY = groundY - branchHeight;
        double branchThickness = Math.Min(40, Math.Max(20, treeHeight / 8)) * 0.35;

        // Position monkey at the specified height (monkeyHeight * yScale) from ground
        double monkeyY = groundY - monkeyHeight * yScale;

        // Monkey body
        double bodyWidth = monkeyScale * 0.8;
        double bodyHeight = monkeyScale * 1.2;

        Ellipse body = new Ellipse
        {
            Width = bodyWidth,
            Height = bodyHeight,
            Fill = new SolidColorBrush(Color.FromRgb(139, 69, 19)), // SaddleBrown
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(body, x - bodyWidth / 2);
        Canvas.SetTop(body, monkeyY); // Position at correct height
        simulationCanvas.Children.Add(body);

        // Monkey head
        double headSize = monkeyScale * 0.9;

        Ellipse head = new Ellipse
        {
            Width = headSize,
            Height = headSize,
            Fill = new SolidColorBrush(Color.FromRgb(160, 82, 45)), // Sienna 
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetLeft(head, x - headSize / 2);
        Canvas.SetTop(head, monkeyY - headSize * 0.8);
        simulationCanvas.Children.Add(head);

        // Face - adjust position relative to head
        Ellipse face = new Ellipse
        {
            Width = headSize * 0.7,
            Height = headSize * 0.6,
            Fill = Brushes.Bisque,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(face, x - headSize * 0.35);
        Canvas.SetTop(face, monkeyY - headSize * 0.7);
        simulationCanvas.Children.Add(face);

        // Eyes - position relative to face
        double eyeSize = headSize * 0.15;

        // Left eye
        Ellipse leftEye = new Ellipse
        {
            Width = eyeSize,
            Height = eyeSize,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(leftEye, x - headSize * 0.25);
        Canvas.SetTop(leftEye, monkeyY - headSize * 0.65);
        simulationCanvas.Children.Add(leftEye);

        // Left pupil
        Ellipse leftPupil = new Ellipse
        {
            Width = eyeSize * 0.6,
            Height = eyeSize * 0.6,
            Fill = Brushes.Black
        };
        Canvas.SetLeft(leftPupil, x - headSize * 0.25 + eyeSize * 0.2);
        Canvas.SetTop(leftPupil, monkeyY - headSize * 0.65 + eyeSize * 0.2);
        simulationCanvas.Children.Add(leftPupil);

        // Right eye
        Ellipse rightEye = new Ellipse
        {
            Width = eyeSize,
            Height = eyeSize,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(rightEye, x + headSize * 0.1);
        Canvas.SetTop(rightEye, monkeyY - headSize * 0.65);
        simulationCanvas.Children.Add(rightEye);

        // Right pupil
        Ellipse rightPupil = new Ellipse
        {
            Width = eyeSize * 0.6,
            Height = eyeSize * 0.6,
            Fill = Brushes.Black
        };
        Canvas.SetLeft(rightPupil, x + headSize * 0.1 + eyeSize * 0.2);
        Canvas.SetTop(rightPupil, monkeyY - headSize * 0.65 + eyeSize * 0.2);
        simulationCanvas.Children.Add(rightPupil);

        // Smile
        Path mouth = new Path
        {
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry mouthGeometry = new PathGeometry();
        PathFigure mouthFigure = new PathFigure();

        mouthFigure.StartPoint = new Point(x - headSize * 0.15, monkeyY - headSize * 0.45);
        ArcSegment arc = new ArcSegment
        {
            Point = new Point(x + headSize * 0.15, monkeyY - headSize * 0.45),
            Size = new Size(headSize * 0.2, headSize * 0.1),
            SweepDirection = SweepDirection.Clockwise
        };
        mouthFigure.Segments.Add(arc);
        mouthGeometry.Figures.Add(mouthFigure);
        mouth.Data = mouthGeometry;
        simulationCanvas.Children.Add(mouth);

        // Ears - position relative to head
        double earSize = headSize * 0.25;

        // Left ear
        Ellipse leftEar = new Ellipse
        {
            Width = earSize,
            Height = earSize,
            Fill = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(leftEar, x - headSize * 0.5 - earSize * 0.3);
        Canvas.SetTop(leftEar, monkeyY - headSize * 0.7);
        simulationCanvas.Children.Add(leftEar);

        // Right ear
        Ellipse rightEar = new Ellipse
        {
            Width = earSize,
            Height = earSize,
            Fill = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        Canvas.SetLeft(rightEar, x + headSize * 0.5 - earSize * 0.7);
        Canvas.SetTop(rightEar, monkeyY - headSize * 0.7);
        simulationCanvas.Children.Add(rightEar);

        // Arms positioned to hang onto branch
        double armWidth = bodyWidth * 0.25;
        double armLength = bodyHeight * 0.6;

        // Create arms reaching up to branch
        Path leftArm = new Path
        {
            Fill = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry leftArmGeometry = new PathGeometry();
        PathFigure leftArmFigure = new PathFigure();

        leftArmFigure.StartPoint = new Point(x - bodyWidth * 0.3, monkeyY + bodyHeight * 0.2);
        leftArmFigure.Segments.Add(new BezierSegment(
            new Point(x - bodyWidth * 0.5, monkeyY),
            new Point(x - bodyWidth * 0.4, branchY + branchThickness),
            new Point(x - bodyWidth * 0.3, branchY + branchThickness / 2),
            true));
        leftArmFigure.Segments.Add(new BezierSegment(
            new Point(x - bodyWidth * 0.2, branchY + branchThickness * 0.8),
            new Point(x - bodyWidth * 0.1, monkeyY + bodyHeight * 0.1),
            new Point(x - bodyWidth * 0.1, monkeyY + bodyHeight * 0.2),
            true));

        leftArmFigure.IsClosed = true;
        leftArmGeometry.Figures.Add(leftArmFigure);
        leftArm.Data = leftArmGeometry;
        simulationCanvas.Children.Add(leftArm);

        // Similar for right arm

        // Legs hanging down
        double legWidth = bodyWidth * 0.25;
        double legLength = bodyHeight * 0.7;

        Rectangle leftLeg = new Rectangle
        {
            Width = legWidth,
            Height = legLength,
            Fill = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            RadiusX = legWidth / 2,
            RadiusY = legWidth / 2,
            RenderTransform = new RotateTransform(15)
        };
        Canvas.SetLeft(leftLeg, x - bodyWidth * 0.4);
        Canvas.SetTop(leftLeg, monkeyY + bodyHeight * 0.8);
        simulationCanvas.Children.Add(leftLeg);

        // Similar for right leg

        // Tail
        Path tail = new Path
        {
            Stroke = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
            StrokeThickness = armWidth * 0.8,
            StrokeEndLineCap = PenLineCap.Round
        };

        PathGeometry tailGeometry = new PathGeometry();
        PathFigure tailFigure = new PathFigure();

        tailFigure.StartPoint = new Point(x, monkeyY + bodyHeight * 0.8);

        BezierSegment bezier = new BezierSegment
        {
            Point1 = new Point(x + bodyWidth * 0.5, monkeyY + bodyHeight * 1.1),
            Point2 = new Point(x + bodyWidth * 0.8, monkeyY + bodyHeight * 0.9),
            Point3 = new Point(x + bodyWidth * 0.9, monkeyY + bodyHeight * 0.5)
        };

        tailFigure.Segments.Add(bezier);
        tailGeometry.Figures.Add(tailFigure);
        tail.Data = tailGeometry;
        simulationCanvas.Children.Add(tail);

        // We don't need to draw an extra branch here as it's now part of the tree
    }
    private void DrawTree(double x, double monkeyHeight, double groundY)
    {
        // Calculate tree height to be significantly taller than the monkey
        double treeHeight = monkeyHeight * 1.5;

        // Tree width logic - make it proportional to height but with reasonable constraints
        double treeWidth = Math.Min(40, Math.Max(20, treeHeight / 8));

        // Improved trunk texture with more realistic gradient
        LinearGradientBrush trunkBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5)
        };
        trunkBrush.GradientStops.Add(new GradientStop(Color.FromRgb(90, 59, 28), 0.0));
        trunkBrush.GradientStops.Add(new GradientStop(Color.FromRgb(110, 70, 33), 0.3));
        trunkBrush.GradientStops.Add(new GradientStop(Color.FromRgb(121, 85, 38), 0.7));
        trunkBrush.GradientStops.Add(new GradientStop(Color.FromRgb(90, 59, 28), 1.0));

        // Draw trunk with slight taper toward top for realism
        Path trunk = new Path
        {
            Fill = trunkBrush,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry trunkGeometry = new PathGeometry();
        PathFigure trunkFigure = new PathFigure();

        // Start at bottom left
        trunkFigure.StartPoint = new Point(x - treeWidth / 2, groundY);

        // Bottom right
        trunkFigure.Segments.Add(new LineSegment(new Point(x + treeWidth / 2, groundY), true));

        // Top right (slight taper)
        trunkFigure.Segments.Add(new LineSegment(new Point(x + treeWidth * 0.4, groundY - treeHeight * 0.85), true));

        // Top left (slight taper)
        trunkFigure.Segments.Add(new LineSegment(new Point(x - treeWidth * 0.4, groundY - treeHeight * 0.85), true));

        trunkFigure.IsClosed = true;
        trunkGeometry.Figures.Add(trunkFigure);
        trunk.Data = trunkGeometry;

        simulationCanvas.Children.Add(trunk);

        // Add realistic bark texture with random patterns
        Random random = new Random(42); // Fixed seed for consistent texture

        for (int i = 0; i < 15; i++)
        {
            double yPos = groundY - treeHeight * 0.05 - (treeHeight * 0.85 * random.NextDouble());
            double width = treeWidth * (0.3 + 0.5 * random.NextDouble());
            double xOffset = (treeWidth - width) * (random.NextDouble() - 0.5);
            double height = treeHeight * (0.02 + 0.03 * random.NextDouble());

            Rectangle barkPiece = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = new SolidColorBrush(Color.FromRgb(
                    (byte)(70 + random.Next(30)),
                    (byte)(45 + random.Next(20)),
                    (byte)(20 + random.Next(15)))),
                Opacity = 0.7 + random.NextDouble() * 0.3
            };

            Canvas.SetLeft(barkPiece, x - width / 2 + xOffset);
            Canvas.SetTop(barkPiece, yPos);
            simulationCanvas.Children.Add(barkPiece);
        }

        // Add main branch closer to shooter for the monkey
        double branchHeight = treeHeight * 0.85; // Position branch higher on trunk
        double branchWidth = treeWidth * 3.5;
        double branchThickness = treeWidth * 0.35;

        // Branch is angled slightly downward toward shooter
        Path branch = new Path
        {
            Fill = trunkBrush,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry branchGeometry = new PathGeometry();
        PathFigure branchFigure = new PathFigure();

        // Branch starts from trunk
        branchFigure.StartPoint = new Point(x, groundY - branchHeight);

        // Create points for a branch that extends toward shooter (left)
        Point branchEnd = new Point(x - branchWidth, groundY - branchHeight + branchWidth * 0.15);
        Point branchBottomEnd = new Point(x - branchWidth, groundY - branchHeight + branchThickness + branchWidth * 0.15);
        Point branchBottom = new Point(x, groundY - branchHeight + branchThickness);

        branchFigure.Segments.Add(new LineSegment(branchEnd, true));
        branchFigure.Segments.Add(new LineSegment(branchBottomEnd, true));
        branchFigure.Segments.Add(new LineSegment(branchBottom, true));

        branchFigure.IsClosed = true;
        branchGeometry.Figures.Add(branchFigure);
        branch.Data = branchGeometry;

        simulationCanvas.Children.Add(branch);

        // Add branch texture lines
        for (int i = 0; i < 5; i++)
        {
            double t = (i + 1) / 6.0;
            Point start = new Point(
                x - t * branchWidth,
                groundY - branchHeight + t * branchWidth * 0.15);

            Line branchLine = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = start.X,
                Y2 = start.Y + branchThickness,
                Stroke = new SolidColorBrush(Color.FromRgb(80, 55, 30)),
                StrokeThickness = 0.8,
                Opacity = 0.6
            };
            simulationCanvas.Children.Add(branchLine);
        }

        // Add some smaller branches on top
        DrawSmallBranch(x, groundY - branchHeight, 30, treeWidth * 1.5, branchThickness * 0.7);
        DrawSmallBranch(x, groundY - branchHeight, 150, treeWidth * 1.8, branchThickness * 0.7);

        // Tree crown/canopy (leaves) - more natural with varied shapes and colors
        // Create different shades of green for realism
        SolidColorBrush darkGreen = new SolidColorBrush(Color.FromRgb(34, 120, 15));
        SolidColorBrush mediumGreen = new SolidColorBrush(Color.FromRgb(50, 130, 30));
        SolidColorBrush lightGreen = new SolidColorBrush(Color.FromRgb(65, 145, 40));

        double leafClusterSize = treeWidth * 3;

        // Several leaf clusters with different shades for a fuller, more realistic look
        Ellipse leafCluster1 = new Ellipse
        {
            Width = leafClusterSize,
            Height = leafClusterSize * 0.8,
            Fill = darkGreen,
            Opacity = 0.9
        };
        Canvas.SetLeft(leafCluster1, x - leafClusterSize / 2);
        Canvas.SetTop(leafCluster1, groundY - treeHeight * 1.1);
        simulationCanvas.Children.Add(leafCluster1);

        Ellipse leafCluster2 = new Ellipse
        {
            Width = leafClusterSize * 0.9,
            Height = leafClusterSize * 0.85,
            Fill = mediumGreen,
            Opacity = 0.85
        };
        Canvas.SetLeft(leafCluster2, x - leafClusterSize * 0.6);
        Canvas.SetTop(leafCluster2, groundY - treeHeight * 1.05);
        simulationCanvas.Children.Add(leafCluster2);

        Ellipse leafCluster3 = new Ellipse
        {
            Width = leafClusterSize * 0.95,
            Height = leafClusterSize * 0.9,
            Fill = lightGreen,
            Opacity = 0.82
        };
        Canvas.SetLeft(leafCluster3, x + leafClusterSize * 0.1);
        Canvas.SetTop(leafCluster3, groundY - treeHeight * 1.08);
        simulationCanvas.Children.Add(leafCluster3);

        Ellipse leafCluster4 = new Ellipse
        {
            Width = leafClusterSize * 0.8,
            Height = leafClusterSize * 0.75,
            Fill = darkGreen,
            Opacity = 0.88
        };
        Canvas.SetLeft(leafCluster4, x - leafClusterSize * 0.1);
        Canvas.SetTop(leafCluster4, groundY - treeHeight * 1.2);
        simulationCanvas.Children.Add(leafCluster4);
    }
    private void DrawSmallBranch(double x, double y, double angle, double length, double thickness)
    {
        // Convert angle to radians
        double radians = angle * Math.PI / 180.0;
        double dx = length * Math.Cos(radians);
        double dy = length * Math.Sin(radians);

        Path branch = new Path
        {
            Fill = new SolidColorBrush(Color.FromRgb(101, 67, 33)),
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        PathGeometry geometry = new PathGeometry();
        PathFigure figure = new PathFigure();

        figure.StartPoint = new Point(x, y);

        // Create points for the branch (as a thin rectangle)
        Point end = new Point(x + dx, y + dy);

        // Create perpendicular vector for thickness
        double perpX = Math.Sin(radians) * thickness / 2;
        double perpY = -Math.Cos(radians) * thickness / 2;

        figure.Segments.Add(new LineSegment(new Point(end.X + perpX, end.Y + perpY), true));
        figure.Segments.Add(new LineSegment(new Point(end.X - perpX, end.Y - perpY), true));
        figure.Segments.Add(new LineSegment(new Point(x, y), true));

        figure.IsClosed = true;
        geometry.Figures.Add(figure);
        branch.Data = geometry;

        simulationCanvas.Children.Add(branch);
    }
}
