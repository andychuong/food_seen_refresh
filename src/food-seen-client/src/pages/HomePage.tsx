import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import { postsApi } from '@/services/api';
import { useGeolocation } from '@/hooks/useGeolocation';
import PostCard from '@/components/features/PostCard';
import RadiusSelector from '@/components/features/RadiusSelector';
import LocationButton from '@/components/features/LocationButton';
import ViewToggle, { type ViewMode } from '@/components/features/ViewToggle';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loader2, AlertCircle } from 'lucide-react';
import { formatDate } from '@/lib/utils';

export default function HomePage() {
  const [radiusKm, setRadiusKm] = useState(10);
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const { location, loading: locationLoading, error: locationError, requestLocation } = useGeolocation();

  const { data, isLoading, error } = useQuery({
    queryKey: ['posts', 'nearby', location?.latitude, location?.longitude, radiusKm],
    queryFn: () =>
      location
        ? postsApi.getNearby(location.latitude, location.longitude, radiusKm, 1, 100)
        : postsApi.getAll(1, 100),
    enabled: !locationLoading,
  });

  const mapCenter: [number, number] = location
    ? [location.latitude, location.longitude]
    : [37.7749, -122.4194]; // Default to SF

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Discover Free Food Near You</h1>
        <p className="text-muted-foreground">
          Find free food events, samples, and promotions happening in your area.
        </p>
      </div>

      {/* Controls bar */}
      <div className="mb-6 p-4 rounded-lg bg-secondary/50">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <div className="flex items-center gap-4 flex-wrap">
            <LocationButton
              loading={locationLoading}
              hasLocation={!!location}
              error={locationError}
              onRequest={requestLocation}
            />
            {location && (
              <RadiusSelector value={radiusKm} onChange={setRadiusKm} />
            )}
          </div>
          <ViewToggle value={viewMode} onChange={setViewMode} />
        </div>

        {/* Location status message */}
        {!location && !locationLoading && locationError && (
          <div className="mt-3 p-3 rounded-md bg-destructive/10 border border-destructive/20">
            <p className="text-sm text-destructive">{locationError}</p>
            <p className="text-xs text-muted-foreground mt-1">
              Showing all events. Enable location to see nearby events sorted by distance.
            </p>
          </div>
        )}
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="flex items-center justify-center py-12 text-destructive">
          <AlertCircle className="h-5 w-5 mr-2" />
          <span>Failed to load posts. Please try again.</span>
        </div>
      )}

      {/* Content based on view mode */}
      {data && (
        <>
          {viewMode === 'list' ? (
            /* List View */
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {data.items.map((post) => (
                <PostCard key={post.id} post={post} />
              ))}
            </div>
          ) : (
            /* Map View */
            <div className="h-[calc(100vh-350px)] min-h-[400px] rounded-lg overflow-hidden border">
              <MapContainer
                center={mapCenter}
                zoom={12}
                className="h-full w-full"
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {data.items.map((post) => (
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
          )}

          {data.items.length === 0 && (
            <div className="text-center py-12 text-muted-foreground">
              <p>No food events found in this area.</p>
              <p className="mt-2">Try increasing the search radius or check back later!</p>
            </div>
          )}

          {data.totalPages > 1 && viewMode === 'list' && (
            <div className="mt-8 flex justify-center">
              <p className="text-muted-foreground">
                Showing {data.items.length} of {data.totalCount} events
              </p>
            </div>
          )}
        </>
      )}
    </div>
  );
}
