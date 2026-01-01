import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { postsApi, categoriesApi } from '@/services/api';
import { useToast } from '@/context/ToastContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import LocationPicker from '@/components/features/LocationPicker';
import { ArrowLeft, Loader2 } from 'lucide-react';
import type { CreatePostRequest } from '@/types';

interface FormErrors {
  title?: string;
  description?: string;
  address?: string;
  location?: string;
  eventDate?: string;
}

export default function CreatePostPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { addToast } = useToast();

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
  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: categoriesApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePostRequest) => postsApi.create(data),
    onSuccess: (post) => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      addToast('Event created successfully!', 'success');
      navigate(`/posts/${post.id}`);
    },
    onError: () => {
      addToast('Failed to create event. Please try again.', 'error');
    },
  });

  const validateField = (field: keyof FormErrors, value: unknown): string | undefined => {
    switch (field) {
      case 'title':
        if (!value || (typeof value === 'string' && value.trim().length === 0)) {
          return 'Title is required';
        }
        if (typeof value === 'string' && value.length > 200) {
          return 'Title must be less than 200 characters';
        }
        break;
      case 'description':
        if (!value || (typeof value === 'string' && value.trim().length === 0)) {
          return 'Description is required';
        }
        if (typeof value === 'string' && value.length < 10) {
          return 'Description must be at least 10 characters';
        }
        break;
      case 'address':
        if (!value || (typeof value === 'string' && value.trim().length === 0)) {
          return 'Address is required';
        }
        break;
      case 'eventDate':
        if (!value) {
          return 'Event date is required';
        }
        if (typeof value === 'string' && new Date(value) < new Date()) {
          return 'Event date must be in the future';
        }
        break;
    }
    return undefined;
  };

  const handleBlur = (field: keyof FormErrors) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
    const error = validateField(field, formData[field as keyof CreatePostRequest]);
    setErrors((prev) => ({ ...prev, [field]: error }));
  };

  const handleCategoryToggle = (categoryId: string) => {
    setSelectedCategories((prev) => {
      const updated = prev.includes(categoryId)
        ? prev.filter((id) => id !== categoryId)
        : [...prev, categoryId];
      setFormData((f) => ({ ...f, categoryIds: updated }));
      return updated;
    });
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {
      title: validateField('title', formData.title),
      description: validateField('description', formData.description),
      address: validateField('address', formData.address),
      eventDate: validateField('eventDate', formData.eventDate),
      location: formData.latitude === 0 && formData.longitude === 0
        ? 'Please select a location on the map'
        : undefined,
    };

    setErrors(newErrors);
    setTouched({ title: true, description: true, address: true, eventDate: true, location: true });

    return !Object.values(newErrors).some((error) => error !== undefined);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateForm()) {
      createMutation.mutate(formData);
    } else {
      addToast('Please fix the form errors before submitting.', 'warning');
    }
  };

  const isValid =
    formData.title.trim() &&
    formData.description.trim() &&
    formData.address.trim() &&
    (formData.latitude !== 0 || formData.longitude !== 0) &&
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
                onBlur={() => handleBlur('title')}
                className={`mt-1 ${touched.title && errors.title ? 'border-destructive' : ''}`}
              />
              {touched.title && errors.title && (
                <p className="text-sm text-destructive mt-1">{errors.title}</p>
              )}
            </div>

            <div>
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                placeholder="Describe the event, what food is available, any requirements..."
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                onBlur={() => handleBlur('description')}
                className={`mt-1 min-h-[120px] ${touched.description && errors.description ? 'border-destructive' : ''}`}
              />
              {touched.description && errors.description && (
                <p className="text-sm text-destructive mt-1">{errors.description}</p>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="eventDate">Event Date & Time *</Label>
                <Input
                  id="eventDate"
                  type="datetime-local"
                  value={formData.eventDate}
                  onChange={(e) => setFormData({ ...formData, eventDate: e.target.value })}
                  onBlur={() => handleBlur('eventDate')}
                  className={`mt-1 ${touched.eventDate && errors.eventDate ? 'border-destructive' : ''}`}
                />
                {touched.eventDate && errors.eventDate && (
                  <p className="text-sm text-destructive mt-1">{errors.eventDate}</p>
                )}
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
                  onAddressChange={(address) => {
                    setFormData({ ...formData, address });
                    setTouched((prev) => ({ ...prev, address: true }));
                  }}
                />
              </div>
              {touched.location && errors.location && (
                <p className="text-sm text-destructive mt-1">{errors.location}</p>
              )}
              {touched.address && errors.address && (
                <p className="text-sm text-destructive mt-1">{errors.address}</p>
              )}
            </div>

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
