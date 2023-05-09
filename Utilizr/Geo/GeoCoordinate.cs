using System;
using System.Collections.Generic;
using System.Text;

namespace Utilizr.Geo
{

    /// <summary>
    /// Extracted from System.Device in .Net 4.5
    /// </summary>
    public class GeoCoordinate
    {
        private double m_latitude = double.NaN;
        private double m_longitude = double.NaN;

        /// <summary>Gets or sets the latitude of the <see cref="T:System.Device.Location.GeoCoordinate" />.</summary>
        /// <returns>Latitude of the location.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <see cref="P:System.Device.Location.GeoCoordinate.Latitude" /> is set outside the valid range.</exception>
        public double Latitude
        {
            get
            {
                return this.m_latitude;
            }
            set
            {
                if (value > 90.0 || value < -90.0)
                    throw new ArgumentOutOfRangeException("Latitude", "The value of the parameter must be from -90.0 to 90.0.");
                this.m_latitude = value;
            }
        }

        /// <summary>Gets or sets the longitude of the <see cref="T:System.Device.Location.GeoCoordinate" />.</summary>
        /// <returns>The longitude.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <see cref="P:System.Device.Location.GeoCoordinate.Longitude" /> is set outside the valid range.</exception>
        public double Longitude
        {
            get
            {
                return this.m_longitude;
            }
            set
            {
                if (value > 180.0 || value < -180.0)
                    throw new ArgumentOutOfRangeException("Longitude", "The value of the parameter must be from -90.0 to 90.0.");
                this.m_longitude = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Device.Location.GeoCoordinate" /> class from latitude, longitude, altitude, horizontal accuracy, vertical accuracy, speed, and course.</summary>
        /// <param name="latitude">The latitude of the location. May range from -90.0 to 90.0.</param>
        /// <param name="longitude">The longitude of the location. May range from -180.0 to 180.0.</param>
        /// <param name="altitude">The altitude in meters. May be negative, 0, positive, or <see cref="F:System.Double.NaN" />, if unknown.</param>
        /// <param name="horizontalAccuracy">The accuracy of the latitude and longitude coordinates, in meters. Must be greater than or equal to 0. If a value of 0 is supplied to this constructor, the <see cref="P:System.Device.Location.GeoCoordinate.HorizontalAccuracy" /> property will be set to <see cref="F:System.Double.NaN" />.</param>
        /// <param name="verticalAccuracy">The accuracy of the altitude, in meters. Must be greater than or equal to 0. If a value of 0 is supplied to this constructor, the <see cref="P:System.Device.Location.GeoCoordinate.VerticalAccuracy" /> property will be set to <see cref="F:System.Double.NaN" />.</param>
        /// <param name="speed">The speed measured in meters per second. May be negative, 0, positive, or <see cref="F:System.Double.NaN" />, if unknown.  A negative speed can indicate moving in reverse.</param>
        /// <param name="course">The direction of travel, rather than orientation. This parameter is measured in degrees relative to true north. Must range from 0 to 360.0, or be <see cref="F:System.Double.NaN" />. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="latitude" />, <paramref name="longitude" />, <paramref name="horizontalAccuracy" />, <paramref name="verticalAccuracy," /> or <paramref name="course" /> is out of range.</exception>
        public GeoCoordinate(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
        /// <summary>
        /// Taken from .Net 4.5
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double GetDistanceTo(GeoCoordinate other)
        {
            if (double.IsNaN(this.Latitude) || double.IsNaN(this.Longitude) || double.IsNaN(other.Latitude) || double.IsNaN(other.Longitude))
            {
                throw new ArgumentException("The coordinate's latitude or longitude is not a number.");
            }
            else
            {
                double latitude = this.Latitude * 0.0174532925199433;
                double longitude = this.Longitude * 0.0174532925199433;
                double num = other.Latitude * 0.0174532925199433;
                double longitude1 = other.Longitude * 0.0174532925199433;
                double num1 = longitude1 - longitude;
                double num2 = num - latitude;
                double num3 = Math.Pow(Math.Sin(num2 / 2), 2) + Math.Cos(latitude) * Math.Cos(num) * Math.Pow(Math.Sin(num1 / 2), 2);
                double num4 = 2 * Math.Atan2(Math.Sqrt(num3), Math.Sqrt(1 - num3));
                double num5 = 6376500 * num4;
                return num5;
            }
        }
    }
}
