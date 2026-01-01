import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import { postsApi } from '@/services/api';
import { useGeolocation } from '@/hooks/useGeolocation';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loader2 } from 'lucide-react';
import { formatDate } from '@/lib/utils';

export default function MapViewPage() {
  const [radiusKm, setRadiusKm] = useState(10);
  const { location, loading: locationLoading } = useGeolocation();

  const defaultCenter: [number, number] = location
    ? [location.latitude, location.longitude]
    : [37.7749, -122.4194]; // Default to SF

  const { data, isLoading } = useQuery({
    queryKey: ['posts', 'nearby', location?.latitude, location?.longitude, radiusKm],
    queryFn: () =>
      location
        ? postsApi.getNearby(location.latitude, location.longitude, radiusKm, 1, 100)
        : postsApi.getAll(1, 100),
    enabled: !locationLoading,
  });

  if (locationLoading || isLoading) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-200px)]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="h-[calc(100vh-200px)] flex flex-col">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold">Map View</h1>
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">Radius:</span>
          <select
            value={radiusKm}
            onChange={(e) => setRadiusKm(Number(e.target.value))}
            className="px-2 py-1 rounded border bg-background"
          >
            <option value={5}>5 km</option>
            <option value={10}>10 km</option>
            <option value={25}>25 km</option>
            <option value={50}>50 km</option>
          </select>
        </div>
      </div>

      <div className="flex-1 rounded-lg overflow-hidden border">
        <MapContainer
          center={defaultCenter}
          zoom={12}
          className="h-full w-full"
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {data?.items.map((post) => (
            <Marker key={post.id} position={[post.latitude, post.longitude]}>
              <Popup>
                <div className="min-w-[200px]">
                  <h3 className="font-semibold mb-1">{post.title}</h3>
                  <p className="text-sm text-gray-600 mb-2">{post.address}</p>
                  <p className="text-sm mb-2">{formatDate(post.eventDate)}</p>
                  <div className="flex flex-wrap gap-1 mb-2">
                    {post.categories.map((cat) => (
                      <Badge key={cat} variant="secondary" className="text-xs">
                        {cat}
                      </Badge>
                    ))}
                  </div>
                  <Link to={`/posts/${post.id}`}>
                    <Button size="sm" className="w-full">View Details</Button>
                  </Link>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      </div>
    </div>
  );
}
