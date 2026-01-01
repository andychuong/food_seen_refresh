import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { postsApi, categoriesApi } from '@/services/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import LocationPicker from '@/components/features/LocationPicker';
import { ArrowLeft, Loader2 } from 'lucide-react';
import type { CreatePostRequest } from '@/types';

export default function CreatePostPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [formData, setFormData] = useState<CreatePostRequest>({
    title: '',
    description: '',
    address: '',
    latitude: 0,
    longitude: 0,
    eventDate: '',
    eventEndDate: '',
    categoryIds: [],
  });

  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: categoriesApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePostRequest) => postsApi.create(data),
    onSuccess: (post) => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      navigate(`/posts/${post.id}`);
    },
  });

  const handleCategoryToggle = (categoryId: string) => {
    setSelectedCategories((prev) => {
      const updated = prev.includes(categoryId)
        ? prev.filter((id) => id !== categoryId)
        : [...prev, categoryId];
      setFormData((f) => ({ ...f, categoryIds: updated }));
      return updated;
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  const isValid =
    formData.title.trim() &&
    formData.description.trim() &&
    formData.address.trim() &&
    formData.latitude !== 0 &&
    formData.longitude !== 0 &&
    formData.eventDate;

  return (
    <div className="max-w-2xl mx-auto">
      <Link to="/" className="inline-flex items-center text-muted-foreground hover:text-foreground mb-6">
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back to events
      </Link>

      <Card>
        <CardHeader>
          <CardTitle>Create New Food Event</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <Label htmlFor="title">Event Title *</Label>
              <Input
                id="title"
                placeholder="e.g., Free Pizza at Community Center"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                className="mt-1"
                required
              />
            </div>

            <div>
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                placeholder="Describe the event, what food is available, any requirements..."
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="mt-1 min-h-[120px]"
                required
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="eventDate">Event Date & Time *</Label>
                <Input
                  id="eventDate"
                  type="datetime-local"
                  value={formData.eventDate}
                  onChange={(e) => setFormData({ ...formData, eventDate: e.target.value })}
                  className="mt-1"
                  required
                />
              </div>
              <div>
                <Label htmlFor="eventEndDate">End Date & Time (optional)</Label>
                <Input
                  id="eventEndDate"
                  type="datetime-local"
                  value={formData.eventEndDate || ''}
                  onChange={(e) => setFormData({ ...formData, eventEndDate: e.target.value || undefined })}
                  className="mt-1"
                />
              </div>
            </div>

            <div>
              <Label>Categories</Label>
              <div className="flex flex-wrap gap-2 mt-2">
                {categories?.map((category) => (
                  <Badge
                    key={category.id}
                    variant={selectedCategories.includes(category.id) ? 'default' : 'outline'}
                    className="cursor-pointer"
                    onClick={() => handleCategoryToggle(category.id)}
                  >
                    {category.name}
                  </Badge>
                ))}
              </div>
            </div>

            <div>
              <Label>Location *</Label>
              <div className="mt-2">
                <LocationPicker
                  latitude={formData.latitude}
                  longitude={formData.longitude}
                  address={formData.address}
                  onLocationChange={(lat, lng) =>
                    setFormData({ ...formData, latitude: lat, longitude: lng })
                  }
                  onAddressChange={(address) => setFormData({ ...formData, address })}
                />
              </div>
            </div>

            {createMutation.error && (
              <p className="text-sm text-destructive">
                Failed to create post. Please try again.
              </p>
            )}

            <div className="flex gap-4">
              <Button
                type="submit"
                disabled={!isValid || createMutation.isPending}
                className="flex-1"
              >
                {createMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Creating...
                  </>
                ) : (
                  'Create Event'
                )}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate('/')}>
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
