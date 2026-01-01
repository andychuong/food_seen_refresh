import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { postsApi } from '@/services/api';
import { useGeolocation } from '@/hooks/useGeolocation';
import PostCard from '@/components/features/PostCard';
import { Button } from '@/components/ui/button';
import { MapPin, Loader2, AlertCircle } from 'lucide-react';

export default function HomePage() {
  const [radiusKm, setRadiusKm] = useState(10);
  const { location, loading: locationLoading, error: locationError, requestLocation } = useGeolocation();

  const { data, isLoading, error } = useQuery({
    queryKey: ['posts', 'nearby', location?.latitude, location?.longitude, radiusKm],
    queryFn: () =>
      location
        ? postsApi.getNearby(location.latitude, location.longitude, radiusKm)
        : postsApi.getAll(),
    enabled: !locationLoading,
  });

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Discover Free Food Near You</h1>
        <p className="text-muted-foreground">
          Find free food events, samples, and promotions happening in your area.
        </p>
      </div>

      {/* Location status */}
      <div className="mb-6 p-4 rounded-lg bg-secondary/50">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <div className="flex items-center">
            <MapPin className="h-5 w-5 mr-2 text-primary" />
            {locationLoading ? (
              <span className="text-muted-foreground">Getting your location...</span>
            ) : location ? (
              <span>
                Showing events within{' '}
                <select
                  value={radiusKm}
                  onChange={(e) => setRadiusKm(Number(e.target.value))}
                  className="inline-block px-2 py-1 rounded border bg-background"
                >
                  <option value={5}>5 km</option>
                  <option value={10}>10 km</option>
                  <option value={25}>25 km</option>
                  <option value={50}>50 km</option>
                </select>
                {' '}of your location
              </span>
            ) : (
              <span className="text-muted-foreground">
                {locationError || 'Enable location to see nearby events'}
              </span>
            )}
          </div>

          {!location && !locationLoading && (
            <Button onClick={requestLocation} variant="outline" size="sm">
              <MapPin className="h-4 w-4 mr-2" />
              Use My Location
            </Button>
          )}
        </div>
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

      {/* Posts grid */}
      {data && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {data.items.map((post) => (
              <PostCard key={post.id} post={post} />
            ))}
          </div>

          {data.items.length === 0 && (
            <div className="text-center py-12 text-muted-foreground">
              <p>No food events found in this area.</p>
              <p className="mt-2">Try increasing the search radius or check back later!</p>
            </div>
          )}

          {data.totalPages > 1 && (
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
