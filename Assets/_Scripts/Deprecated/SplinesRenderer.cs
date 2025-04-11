//if (firstSpline == null) // Spline Not Found, Starting...
//{
//    if (intersections.ContainsKey(firstWorldPosition)) // ...From an Intersection...
//    {
//        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To an Intersection");

//            Intersection intersection1 = intersections[firstWorldPosition];
//            ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection1);

//            Intersection intersection2 = intersections[secondWorldPosition];
//            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection2);
//        }
//        else
//        {
//            if (secondSpline == null) // ...To Nothing *
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To Nothing");
//                Intersection intersection = intersections[firstWorldPosition];
//                ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);
//            }
//            else // ...To a Spline...
//            {
//                Intersection intersection = intersections[firstWorldPosition];
//                Spline newSpline = ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);

//                if (secondIndex == -1) // ...at the Middle *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Middle");
//                    CreateTripleIntersectionByCrossingStraightSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                }
//                else // ...at the Edge...
//                {
//                    if (isSecondIntersection) // ...and They're Not Collinear *
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Not Collinear");
//                        CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                    }
//                    else // ...and They're Collinear...
//                    {
//                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
//                        }
//                        else // ...and Is a Straight Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
//                            FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
//                        }
//                    }
//                }
//            }
//        }
//    }
//    else // ...From Nothing... ***
//    {
//        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To an Intersection");
//            Intersection intersection = intersections[secondWorldPosition];
//            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//        }
//        else
//        {
//            if (secondSpline == null) // ...To Nothing *
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To Nothing");
//                StartNewSpline(firstWorldPosition, secondWorldPosition);
//            }
//            else // ...To a Spline...
//            {
//                Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);

//                if (secondIndex == -1) // ...at the Middle *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Middle");
//                    CreateTripleIntersectionByCrossingStraightSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                }
//                else // ...at the Edge...
//                {
//                    if (isSecondIntersection) // ...and They're Not Collinear *
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Not Collinear");
//                        CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                    }
//                    else // ...and They're Collinear...
//                    {
//                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
//                            // Do Nothing?
//                        }
//                        else // ...and Is a Straight Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
//                            FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
//else // Spline Found, Starting...
//{
//    if (firstIndex == -1) // ...From the Middle...
//    {
//        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//        {
//            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To an Intersection");

//            Spline newSpline = CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);
//            ClearSpline(newSpline);

//            Intersection intersection = intersections[secondWorldPosition];
//            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//        }
//        else
//        {
//            if (secondSpline == null) // ...To Nothing *
//            {
//                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To Nothing");
//                CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);
//            }
//            else // ...To a Spline...
//            {
//                Spline newSpline = CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);

//                if (secondIndex == -1) // ...at the Middle *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > at the Middle");
//                    ClearSpline(newSpline);
//                    CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
//                }
//                else // ...at the Edge...
//                {
//                    if (isSecondIntersection) // ...and They're Not Collinear *
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > at the Edge > and They're Not Collinear");
//                        CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                    }
//                    else // ...and They're Collinear...
//                    {
//                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > and They're Collinear > but Is an Intersection Spline");
//                            AddKnotToSpline(newSpline, 1, secondWorldPosition);
//                        }
//                        else // ...and Is a Straight Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > and They're Collinear > and Is a Straight Spline");
//                            FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
//                        }
//                    }
//                }
//            }
//        }
//    }
//    else // ...From the Edge...
//    {
//        if (isFirstIntersection) // ...with a Change of Direction...
//        {
//            if (IsIntersectionSpline(firstSpline)) // ...and From an Intersection Spline...
//            {
//                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To an Intersection");
//                    Intersection intersection = intersections[secondWorldPosition];
//                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//                }
//                else
//                {
//                    if (secondSpline == null) // ...To Nothing *
//                    {
//                        if (firstNeighbors.Count > 2) // ...but From a Conflictive Point *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To Nothing > but From a Conflictive Point");
//                            CreateTripleIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition);
//                        }
//                        else  // ...and From a Single Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To Nothing > and From a Single Spline");
//                            StartNewSpline(firstWorldPosition, secondWorldPosition);
//                        }
//                    }
//                    else // ...To a Spline...
//                    {
//                        if (secondIndex == -1) // ...at the Middle *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Middle");
//                            Spline newSpline = CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
//                        }
//                        else // ...at the Edge...
//                        {
//                            if (isSecondIntersection) // ...and They're Not Collinear...
//                            {
//                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Not Collinear > but Is an Intersection Spline");
//                                    StartNewSpline(firstWorldPosition, secondWorldPosition);
//                                }
//                                else // ...but is a Straight Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Not Collinear > but is a Straight Spline");
//                                    CreateIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition, secondSpline, secondIndex);
//                                }
//                            }
//                            else // ...and They're Collinear *
//                            {
//                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Collinear");
//                                Spline newnSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);
//                                FuseStraightSplines(newnSpline, 1, secondSpline, secondIndex);
//                            }
//                        }
//                    }
//                }
//            }
//            else // ...and From a Straight Spline...
//            {
//                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To an Intersection");
//                    CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                    Intersection intersection = intersections[secondWorldPosition];
//                    Spline newSpline = ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//                    ClearSpline(newSpline);
//                }
//                else
//                {
//                    if (secondSpline == null) // ...To Nothing...
//                    {
//                        if (firstNeighbors.Count > 2) // ...but From a Conflictive Point *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To Nothing > but From a Conflictive Point");
//                            CreateTripleIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition);
//                        }
//                        else  // ...and From a Single Spline *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To Nothing > and From a Single Spline");
//                            CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                        }
//                    }
//                    else // ...To a Spline...
//                    {
//                        if (secondIndex == -1) // ...at the Middle *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Middle");
//                            CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                            Spline newSpline = CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
//                            ClearSpline(newSpline);
//                        }
//                        else // ...at the Edge...
//                        {
//                            if (isSecondIntersection) // ...and They're Not Collinear *
//                            {
//                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Not Collinear");
//                                CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                            }
//                            else // ...and They're Collinear...
//                            {
//                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
//                                    CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                                }
//                                else // ...and Is a Straight Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
//                                    CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        else // ...w/o a Change of Direction...
//        {
//            if (IsIntersectionSpline(firstSpline)) // ...and From an Intersection Spline...
//            {
//                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To an Intersection");

//                    Intersection intersection = intersections[secondWorldPosition];
//                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
//                }
//                else
//                {
//                    if (secondSpline == null) // ...To Nothing *
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To Nothing");
//                        StartNewSpline(firstWorldPosition, secondWorldPosition);
//                    }
//                    else // ...To a Spline...
//                    {
//                        if (secondIndex == -1) // ...at the Middle *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Middle");
//                            CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
//                        }
//                        else // ...at the Edge...
//                        {
//                            if (isSecondIntersection) // ...and They're Not Collinear *
//                            {
//                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > and They're Not Collinear");
//                                CreateIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition, secondSpline, secondIndex);
//                            }
//                            else // ...and They're Collinear...
//                            {
//                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > but Is an Intersection Spline");
//                                    StartNewSpline(firstWorldPosition, secondWorldPosition);
//                                }
//                                else // ...and Is a Straight Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > and Is a Straight Spline");
//                                    Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);
//                                    FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            else // ...and From a Straight Spline...
//            {
//                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
//                {
//                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To an Intersection");

//                    Intersection intersection = intersections[secondWorldPosition];
//                    Spline newSpline = ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);

//                    FuseStraightSplines(firstSpline, firstIndex, newSpline, 1);
//                }
//                else
//                {
//                    if (secondSpline == null) // ...To Nothing *
//                    {
//                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To Nothing");
//                        AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
//                    }
//                    else // ...To a Spline...
//                    {
//                        if (secondIndex == -1) // ...at the Middle *
//                        {
//                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Middle");
//                            CreateTripleIntersectionByCrossingStraightSplines(firstSpline, firstIndex, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                        }
//                        else // ...at the Edge...
//                        {
//                            if (isSecondIntersection) // ...and They're Not Collinear *
//                            {
//                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Not Collinear");
//                                AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
//                                CreateIntersectionByFusingStraighSplines(firstSpline, firstIndex, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
//                            }
//                            else // ...and They're Collinear...
//                            {
//                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > but Is an Intersection Spline");
//                                    AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
//                                }
//                                else // ...and Is a Straight Spline *
//                                {
//                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and Is a Straight Spline");
//                                    FuseStraightSplines(firstSpline, firstIndex, secondSpline, secondIndex);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//}