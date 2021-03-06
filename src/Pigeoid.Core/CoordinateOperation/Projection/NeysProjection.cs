﻿using System;
using System.Diagnostics.Contracts;
using Vertesaur;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class NeysProjection : LambertConicConformal2Sp
    {

        private const double MaximumLatitude = (89.0 + (59.0 / 60) + (58.0 / 3600)) * Math.PI / 180.0;
        private const double TwoPi = Math.PI + Math.PI;

        public NeysProjection(
            GeographicCoordinate geographicOrigin,
            double standardParallel,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(
            new GeographicCoordinate(
                geographicOrigin.Latitude,
                geographicOrigin.Longitude > Math.PI ? (geographicOrigin.Longitude - TwoPi) : geographicOrigin.Longitude
                ),
            geographicOrigin.Latitude >= 0 ? standardParallel : -standardParallel,
            geographicOrigin.Latitude >= 0 ? MaximumLatitude : -MaximumLatitude,
            falseProjectedOffset,
            spheroid
        ) {
            Contract.Requires(spheroid != null);
        }

    }
}
