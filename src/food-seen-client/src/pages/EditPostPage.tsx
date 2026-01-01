import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
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
import type { UpdatePostRequest } from '@/types';

export default function EditPostPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [formData, setFormData] = useState<UpdatePostRequest>({
    title: '',
    description: '',
    address: '',
    latitude: 0,
    longitude: 0,
    eventDate: '',
    eventEndDate: '',
    isActive: true,
    categoryIds: [],
  });

  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);

  const { data: post, isLoading: postLoading } = useQuery({
    queryKey: ['post', id],
    queryFn: () => postsApi.getById(id!),
    enabled: !!id,
  });

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: categoriesApi.getAll,
  });

  useEffect(() => {
    if (post && categories) {
      const categoryIds = categories
        .filter((c) => post.categories.includes(c.name))
        .map((c) => c.id);

      setFormData({
        title: post.title,
        description: post.description,
        address: post.address,
        latitude: post.latitude,
        longitude: post.longitude,
        eventDate: new Date(post.eventDate).toISOString().slice(0, 16),
        eventEndDate: post.eventEndDate
          ? new Date(post.eventEndDate).toISOString().slice(0, 16)
          : undefined,
        isActive: post.isActive,
        categoryIds,
      });
      setSelectedCategories(categoryIds);
    }
  }, [post, categories]);

  const updateMutation = useMutation({
    mutationFn: (data: UpdatePostRequest) => postsApi.update(id!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      queryClient.invalidateQueries({ queryKey: ['post', id] });
      navigate(`/posts/${id}`);
    },
  });

  const handleCategoryToggle = (categoryId: string) => {
    setSelectedCategories((prev) => {
      const updated = prev.includes(categoryId)
        ? prev.filter((cid) => cid !== categoryId)
        : [...prev, categoryId];
      setFormData((f) => ({ ...f, categoryIds: updated }));
      return updated;
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate(formData);
  };

  if (postLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!post) {
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

  const isValid =
    formData.title?.trim() &&
    formData.description?.trim() &&
    formData.address?.trim() &&
    formData.latitude !== 0 &&
    formData.longitude !== 0 &&
    formData.eventDate;

  return (
    <div className="max-w-2xl mx-auto">
      <Link to={`/posts/${id}`} className="inline-flex items-center text-muted-foreground hover:text-foreground mb-6">
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back to event
      </Link>

      <Card>
        <CardHeader>
          <CardTitle>Edit Event</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <Label htmlFor="title">Event Title *</Label>
              <Input
                id="title"
                placeholder="e.g., Free Pizza at Community Center"
                value={formData.title || ''}
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
                value={formData.description || ''}
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
                  value={formData.eventDate || ''}
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

            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4"
              />
              <Label htmlFor="isActive">Event is active</Label>
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
                  latitude={formData.latitude || 0}
                  longitude={formData.longitude || 0}
                  address={formData.address || ''}
                  onLocationChange={(lat, lng) =>
                    setFormData({ ...formData, latitude: lat, longitude: lng })
                  }
                  onAddressChange={(address) => setFormData({ ...formData, address })}
                />
              </div>
            </div>

            {updateMutation.error && (
              <p className="text-sm text-destructive">
                Failed to update post. Please try again.
              </p>
            )}

            <div className="flex gap-4">
              <Button
                type="submit"
                disabled={!isValid || updateMutation.isPending}
                className="flex-1"
              >
                {updateMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Saving...
                  </>
                ) : (
                  'Save Changes'
                )}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate(`/posts/${id}`)}>
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
