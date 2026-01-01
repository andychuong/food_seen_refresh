import { useQuery } from '@tanstack/react-query';
import { useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import { postsApi, categoriesApi } from '@/services/api';
import { useGeolocation } from '@/hooks/useGeolocation';
import PostCard from '@/components/features/PostCard';
import RadiusSelector from '@/components/features/RadiusSelector';
import LocationButton from '@/components/features/LocationButton';
import ViewToggle, { type ViewMode } from '@/components/features/ViewToggle';
import Pagination from '@/components/features/Pagination';
import SearchBar from '@/components/features/SearchBar';
import CategoryFilter from '@/components/features/CategoryFilter';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loader2, AlertCircle } from 'lucide-react';
import { formatDate } from '@/lib/utils';

const PAGE_SIZE = 12;

export default function HomePage() {
  const [radiusKm, setRadiusKm] = useState(10);
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [page, setPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [activeSearch, setActiveSearch] = useState('');
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const { location, loading: locationLoading, error: locationError, requestLocation } = useGeolocation();

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: categoriesApi.getAll,
  });

  const { data, isLoading, error } = useQuery({
    queryKey: ['posts', 'nearby', location?.latitude, location?.longitude, radiusKm, page, activeSearch],
    queryFn: () => {
      if (activeSearch) {
        return postsApi.search(activeSearch, page, PAGE_SIZE);
      }
      return location
        ? postsApi.getNearby(location.latitude, location.longitude, radiusKm, page, PAGE_SIZE)
        : postsApi.getAll(page, PAGE_SIZE);
    },
    enabled: !locationLoading,
  });

  // Filter by selected categories (client-side for simplicity)
  const filteredItems = useMemo(() => {
    if (!data?.items || selectedCategories.length === 0) {
      return data?.items || [];
    }
    return data.items.filter((post) =>
      post.categories.some((cat) =>
        selectedCategories.some((selectedId) => {
          const category = categories?.find((c) => c.id === selectedId);
          return category?.name === cat;
        })
      )
    );
  }, [data?.items, selectedCategories, categories]);

  const mapCenter: [number, number] = location
    ? [location.latitude, location.longitude]
    : [37.7749, -122.4194]; // Default to SF

  const handleSearch = (query: string) => {
    setActiveSearch(query);
    setPage(1);
  };

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Discover Free Food Near You</h1>
        <p className="text-muted-foreground">
          Find free food events, samples, and promotions happening in your area.
        </p>
      </div>

      {/* Search bar */}
      <div className="mb-4">
        <SearchBar
          value={searchQuery}
          onChange={setSearchQuery}
          onSearch={handleSearch}
          placeholder="Search events by title, description, or location..."
        />
      </div>

      {/* Category filter */}
      {categories && categories.length > 0 && (
        <div className="mb-4">
          <CategoryFilter
            categories={categories}
            selected={selectedCategories}
            onChange={setSelectedCategories}
          />
        </div>
      )}

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

      {/* Active search indicator */}
      {activeSearch && (
        <div className="mb-4 flex items-center gap-2">
          <span className="text-sm text-muted-foreground">
            Showing results for: <strong>"{activeSearch}"</strong>
          </span>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              setSearchQuery('');
              setActiveSearch('');
            }}
          >
            Clear
          </Button>
        </div>
      )}

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
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredItems.map((post) => (
                  <PostCard key={post.id} post={post} />
                ))}
              </div>

              {filteredItems.length === 0 && (
                <div className="text-center py-12 text-muted-foreground">
                  <p>No food events found.</p>
                  <p className="mt-2">Try adjusting your search or filters!</p>
                </div>
              )}

              {/* Pagination */}
              {data.totalPages > 1 && (
                <div className="mt-8 space-y-4">
                  <Pagination
                    currentPage={data.page}
                    totalPages={data.totalPages}
                    onPageChange={handlePageChange}
                    hasPreviousPage={data.hasPreviousPage}
                    hasNextPage={data.hasNextPage}
                  />
                  <p className="text-center text-sm text-muted-foreground">
                    Showing {(data.page - 1) * data.pageSize + 1} - {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} events
                  </p>
                </div>
              )}
            </>
          ) : (
            /* Map View */
            <div className="h-[calc(100vh-450px)] min-h-[400px] rounded-lg overflow-hidden border">
              <MapContainer
                center={mapCenter}
                zoom={12}
                className="h-full w-full"
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {filteredItems.map((post) => (
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
        </>
      )}
    </div>
  );
}
