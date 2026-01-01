import { Link } from 'react-router-dom';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { MapPin, Calendar, User } from 'lucide-react';
import { formatDate, formatDistance } from '@/lib/utils';
import type { Post } from '@/types';

interface PostCardProps {
  post: Post;
}

export default function PostCard({ post }: PostCardProps) {
  return (
    <Card className="flex flex-col h-full hover:shadow-md transition-shadow">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between">
          <CardTitle className="text-lg line-clamp-2">{post.title}</CardTitle>
          {post.distanceKm !== undefined && (
            <Badge variant="secondary" className="ml-2 shrink-0">
              {formatDistance(post.distanceKm)}
            </Badge>
          )}
        </div>
      </CardHeader>

      <CardContent className="flex-1">
        <p className="text-muted-foreground text-sm line-clamp-3 mb-4">
          {post.description}
        </p>

        <div className="space-y-2 text-sm">
          <div className="flex items-center text-muted-foreground">
            <MapPin className="h-4 w-4 mr-2 shrink-0" />
            <span className="line-clamp-1">{post.address}</span>
          </div>

          <div className="flex items-center text-muted-foreground">
            <Calendar className="h-4 w-4 mr-2 shrink-0" />
            <span>{formatDate(post.eventDate)}</span>
          </div>

          <div className="flex items-center text-muted-foreground">
            <User className="h-4 w-4 mr-2 shrink-0" />
            <span>{post.authorUsername}</span>
          </div>
        </div>

        {post.categories.length > 0 && (
          <div className="flex flex-wrap gap-1 mt-3">
            {post.categories.map((category) => (
              <Badge key={category} variant="outline" className="text-xs">
                {category}
              </Badge>
            ))}
          </div>
        )}
      </CardContent>

      <CardFooter>
        <Link to={`/posts/${post.id}`} className="w-full">
          <Button variant="outline" className="w-full">
            View Details
          </Button>
        </Link>
      </CardFooter>
    </Card>
  );
}
