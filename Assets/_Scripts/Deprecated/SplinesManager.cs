//using System;
//using UnityEngine.Splines;

//if (spline == null) // Spline Not Found...
//{
//    if (intersections.ContainsKey(firstWorldPosition)) // ...From an Intersection
//    {
//        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > Double Expansion");

//            Intersection intersection1 = intersections[firstWorldPosition];
//            ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection1);

//            Intersection intersection2 = intersections[secondWorldPosition];
//            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection2);
//        }
//        else
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > Expansion (From)");
//            Intersection intersection = intersections[firstWorldPosition];
//            ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);
//        }
//    }
//    else if (intersections.ContainsKey(secondWorldPosition))
//    {
//        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > Expansion (Into)");
//        Intersection intersection = intersections[secondWorldPosition];
//        ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//    }
//    else
//    {
//        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > New Spline");
//        Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);

//        if (isSecondIntersection) // ...and Adapt 2 an Intersection
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > Intersection (Fusion)");
//            IntersectionFusing2StraighSplines(newSpline, 1, firstWorldPosition, secondWorldPosition);
//        }
//    }
//}
//else // Spline Found, Building...
//{
//    if (index != -1) // ...on the Edge...
//    {
//        if (IsIntersectionSpline(spline)) // ...from an Intersection Spline to...
//        {
//            if (intersections.ContainsKey(secondWorldPosition)) // ...an Intersection
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Edge > From an Intersection > Expansion (Into)");
//                Intersection intersection = intersections[secondWorldPosition];
//                ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//            }
//            else // ...a New Spline
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Edge > From an Intersection > New Spline");
//                StartNewSpline(firstWorldPosition, secondWorldPosition);
//            }
//        }
//        else // ...from a Straight Spline to...
//        {
//            if (intersections.ContainsKey(secondWorldPosition)) // ...an Intersection
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Edge > From a Straight > Expansion (Into)");
//                Intersection intersection = intersections[secondWorldPosition];
//                ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//            }
//            else
//            {
//                if (isFirstIntersection || isSecondIntersection)
//                {
//                    if (isFirstIntersection) // ...and Form an Intersection
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Intersection");
//                        IntersectionFromStraightSpline(spline, index, firstWorldPosition, secondWorldPosition);
//                    }
//                    if (isSecondIntersection)
//                    {
//                        if (secondNeighbors.Count < 3) // ...and Adapt 2 an Intersection
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Intersection (Fusion)");
//                            IntersectionFusing2StraighSplines(spline, index, firstWorldPosition, secondWorldPosition);
//                        }
//                        else
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Triple Intersection");
//                            TripleIntersectionCrossing2StraightSplines(spline, index, firstWorldPosition, secondWorldPosition);
//                        }
//                    }
//                }
//                else
//                {
//                    (Spline toSpline, int toIndex) = FindKnotAndSpline(secondWorldPosition);

//                    if (toSpline != null)
//                    {
//                        if (toIndex != -1)
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Fuse Splines");
//                            FuseSplines(spline, index, toSpline, toIndex);
//                        }
//                        else
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Triple Intersection");
//                            TripleIntersectionCrossing2StraightSplines(spline, index, firstWorldPosition, secondWorldPosition);
//                        }
//                    }
//                    else // ...an Empty Space...
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Continue");
//                        AddKnotToSpline(spline, index, secondWorldPosition);
//                    }
//                }
//            }
//        }
//    }
//    else // ...on the Middle
//    {
//        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Middle > Triple Intersection");
//        TripleIntersectionCrossing2StraightSplines(spline, index, firstWorldPosition, secondWorldPosition);
//    }
//}