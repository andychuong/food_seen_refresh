import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { postsApi } from '@/services/api';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { MapPin, Calendar, User, ArrowLeft, Loader2 } from 'lucide-react';
import { formatDate } from '@/lib/utils';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';

export default function PostDetailPage() {
  const { id } = useParams<{ id: string }>();

  const { data: post, isLoading, error } = useQuery({
    queryKey: ['post', id],
    queryFn: () => postsApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="text-center py-12">
        <p className="text-destructive">Post not found</p>
        <Link to="/" className="mt-4 inline-block">
          <Button variant="outline">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Home
          </Button>
        </Link>
      </div>
    );
  }

  return (
    <div>
      <Link to="/" className="inline-flex items-center text-muted-foreground hover:text-foreground mb-6">
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back to events
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Event Details */}
        <div>
          <h1 className="text-3xl font-bold mb-4">{post.title}</h1>

          <div className="flex flex-wrap gap-2 mb-6">
            {post.categories.map((category) => (
              <Badge key={category}>{category}</Badge>
            ))}
          </div>

          <Card className="mb-6">
            <CardContent className="pt-6 space-y-4">
              <div className="flex items-start">
                <MapPin className="h-5 w-5 mr-3 mt-0.5 text-primary shrink-0" />
                <div>
                  <p className="font-medium">Location</p>
                  <p className="text-muted-foreground">{post.address}</p>
                </div>
              </div>

              <div className="flex items-start">
                <Calendar className="h-5 w-5 mr-3 mt-0.5 text-primary shrink-0" />
                <div>
                  <p className="font-medium">Date & Time</p>
                  <p className="text-muted-foreground">
                    {formatDate(post.eventDate)}
                    {post.eventEndDate && ` - ${formatDate(post.eventEndDate)}`}
                  </p>
                </div>
              </div>

              <div className="flex items-start">
                <User className="h-5 w-5 mr-3 mt-0.5 text-primary shrink-0" />
                <div>
                  <p className="font-medium">Posted by</p>
                  <p className="text-muted-foreground">{post.authorUsername}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <div>
            <h2 className="text-xl font-semibold mb-3">About this event</h2>
            <p className="text-muted-foreground whitespace-pre-wrap">{post.description}</p>
          </div>
        </div>

        {/* Map */}
        <div className="h-[400px] lg:h-full min-h-[400px] rounded-lg overflow-hidden border">
          <MapContainer
            center={[post.latitude, post.longitude]}
            zoom={15}
            className="h-full w-full"
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <Marker position={[post.latitude, post.longitude]}>
              <Popup>
                <strong>{post.title}</strong>
                <br />
                {post.address}
              </Popup>
            </Marker>
          </MapContainer>
        </div>
      </div>
    </div>
  );
}
