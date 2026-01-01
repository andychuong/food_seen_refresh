import { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { MapPin } from 'lucide-react';
import { useGeolocation } from '@/hooks/useGeolocation';

interface LocationPickerProps {
  latitude: number;
  longitude: number;
  address: string;
  onLocationChange: (lat: number, lng: number) => void;
  onAddressChange: (address: string) => void;
}

function MapClickHandler({ onLocationSelect }: { onLocationSelect: (lat: number, lng: number) => void }) {
  useMapEvents({
    click: (e) => {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });
  return null;
}

export default function LocationPicker({
  latitude,
  longitude,
  address,
  onLocationChange,
  onAddressChange,
}: LocationPickerProps) {
  const { location: userLocation, requestLocation } = useGeolocation();
  const [mapCenter, setMapCenter] = useState<[number, number]>([latitude || 37.7749, longitude || -122.4194]);

  useEffect(() => {
    if (latitude && longitude) {
      setMapCenter([latitude, longitude]);
    } else if (userLocation) {
      setMapCenter([userLocation.latitude, userLocation.longitude]);
      onLocationChange(userLocation.latitude, userLocation.longitude);
    }
  }, [userLocation]);

  const handleMapClick = (lat: number, lng: number) => {
    onLocationChange(lat, lng);
    setMapCenter([lat, lng]);
  };

  const handleUseMyLocation = () => {
    if (userLocation) {
      onLocationChange(userLocation.latitude, userLocation.longitude);
      setMapCenter([userLocation.latitude, userLocation.longitude]);
    } else {
      requestLocation();
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <Label htmlFor="address">Address</Label>
        <Input
          id="address"
          placeholder="Enter the event address"
          value={address}
          onChange={(e) => onAddressChange(e.target.value)}
          className="mt-1"
        />
      </div>

      <div className="flex gap-4">
        <div className="flex-1">
          <Label htmlFor="latitude">Latitude</Label>
          <Input
            id="latitude"
            type="number"
            step="any"
            value={latitude || ''}
            onChange={(e) => onLocationChange(Number(e.target.value), longitude)}
            className="mt-1"
          />
        </div>
        <div className="flex-1">
          <Label htmlFor="longitude">Longitude</Label>
          <Input
            id="longitude"
            type="number"
            step="any"
            value={longitude || ''}
            onChange={(e) => onLocationChange(latitude, Number(e.target.value))}
            className="mt-1"
          />
        </div>
      </div>

      <Button type="button" variant="outline" onClick={handleUseMyLocation} className="w-full">
        <MapPin className="h-4 w-4 mr-2" />
        Use My Current Location
      </Button>

      <div className="h-[300px] rounded-lg overflow-hidden border">
        <MapContainer
          center={mapCenter}
          zoom={13}
          className="h-full w-full"
          key={`${mapCenter[0]}-${mapCenter[1]}`}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <MapClickHandler onLocationSelect={handleMapClick} />
          {latitude && longitude && (
            <Marker position={[latitude, longitude]} />
          )}
        </MapContainer>
      </div>
      <p className="text-sm text-muted-foreground">Click on the map to set the event location</p>
    </div>
  );
}
