// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;

public static class DeadReckoning
{
    /// <summary>
    /// Earth radius in meters
    /// </summary>
    public static double earthRadiusMeters = 6356752.3142; //6378137.0

    /// <summary>
    /// Converts angle from degrees to radians
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns>angle as double in radians</returns>
    public static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Converts angle from radians to degrees
    /// </summary>
    /// <param name="radians"></param>
    /// <returns>angle as double in degrees</returns>
    public static double ToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    /// <summary>
    /// Calculates distance between 2 locations in latitude and longitude along a great circle of the earth in meters
    /// </summary>
    /// <param name="lon1"></param>
    /// <param name="lat1"></param>
    /// <param name="lon2"></param>
    /// <param name="lat2"></param>
    /// <returns>distance in meters</returns>
    public static double DistanceFromLatLon(double lon1, double lat1, double lon2, double lat2)
    {
        var radLon1 = DeadReckoning.ToRadians(lon1);
        var radLat1 = DeadReckoning.ToRadians(lat1);
        var radLon2 = DeadReckoning.ToRadians(lon2);
        var radLat2 = DeadReckoning.ToRadians(lat2);
        return Math.Acos(Math.Sin(radLat1) * Math.Sin(radLat2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radLon2 - radLon1)) * DeadReckoning.earthRadiusMeters;
    }

    /// <summary>
    /// Calculates predictive distance in meters for a given timespan from current position, heading, and speed 
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="timespan"></param>
    /// <param name="currentPoint"></param>
    /// <param name="headingDegrees"></param>
    /// <returns>distance in meters</returns>
    public static double[] DeadReckoningPoint(double speed, double timespan, double[] currentPoint, double headingDegrees)
    {
        var predictiveDistance = speed * timespan;
        return MoveByDistanceAndHeading(currentPoint, predictiveDistance, headingDegrees);
    }

    /// <summary>
    /// Calculates the moved position by distance in meters and heading in degrees from the given point 
    /// </summary>
    /// <param name="currentPoint"></param>
    /// <param name="distanceMeters"></param>
    /// <param name="headingDegrees"></param>
    /// <returns>latitude and longitude as elements of an array of doubles</returns>
    public static double[] MoveByDistanceAndHeading(double[] currentPoint, double distanceMeters, double headingDegrees)
    {
        var distRatio = distanceMeters / DeadReckoning.earthRadiusMeters;
        var distRatioSine = Math.Sin(distRatio);
        var distRatioCosine = Math.Cos(distRatio);

        var startLonRad = DeadReckoning.ToRadians(currentPoint[0]);
        var startLatRad = DeadReckoning.ToRadians(currentPoint[1]);

        var startLatCos = Math.Cos(startLatRad);
        var startLatSin = Math.Sin(startLatRad);

        var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(DeadReckoning.ToRadians(headingDegrees))));
        var endLonRads = startLonRad + Math.Atan2(Math.Sin(DeadReckoning.ToRadians(headingDegrees)) * distRatioSine * startLatCos, distRatioCosine - startLatSin * Math.Sin(endLatRads));

        var newLong = DeadReckoning.ToDegrees(endLonRads);
        var newLat = DeadReckoning.ToDegrees(endLatRads);
        var newPoint = new double[2];
        newPoint[0] = newLong;
        newPoint[1] = newLat;
        return newPoint;
    }

    /// <summary>
    /// Calculates the initial bearing from the origin point to the destination point.
    /// Example:
    /// const p1 = new LatLon(52.205, 0.119);
    ///const p2 = new LatLon(48.857, 2.351);
    ///const b1 = p1.initialBearingTo(p2); // 156.2°
    /// </summary>
    /// <param name="orgPoint"></param>
    /// <param name="dstPoint"></param>
    /// <returns>Initial bearing in degrees from north (0°..360°)</returns>
    public static double InitialBearingTo(double[] orgPoint, double[] dstPoint)
    {
        if (orgPoint[0] == dstPoint[0] && orgPoint[1] == dstPoint[1])
        {
            return double.NaN;
        }

        // see mathforum.org/library/drmath/view/55417.html for derivation

        double phi1 = ToRadians(orgPoint[1]);
        double phi2 = ToRadians(dstPoint[1]);
        double deltaLamda = ToRadians(dstPoint[0] - orgPoint[0]);

        double x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLamda);
        double y = Math.Sin(deltaLamda) * Math.Cos(phi2);
        double theta = Math.Atan2(y, x);

        double bearing = ToDegrees(theta);

        return Wrap360(bearing);
    }

    /// <summary>
    /// Constrains degrees to range 0..360 (e.g. for bearings); -1 => 359, 361 => 1
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns>degrees within range 0..360</returns>
    public static double Wrap360(double degrees)
    {
        if (0 <= degrees && degrees < 360) return degrees; // avoid rounding due to arithmetic ops if within range
        return (degrees % 360 + 360) % 360; // sawtooth wave p:360, a:360
    }

    /// <summary>
    /// Calculates an incremental moved position from the current location given a maximum distance to move.
    /// Calls MoveByDistanceAndHeading()
    /// </summary>
    /// <param name="targetLocation"></param>
    /// <param name="currentLocation"></param>
    /// <param name="maxDistanceDelta"></param>
    /// <returns>latitude and longitude as elements of an array of doubles</returns>
    public static double[] MoveTowards(double[] targetLocation, double[] currentLocation, double maxDistanceDelta)
    {
        double iBearing = InitialBearingTo(currentLocation, targetLocation);
        double distance = DistanceFromLatLon(currentLocation[0], currentLocation[1], targetLocation[0], targetLocation[1]);
        if (distance >= maxDistanceDelta)
        {
            distance = maxDistanceDelta;
        }
        return MoveByDistanceAndHeading(currentLocation, distance, iBearing);
    }
}
