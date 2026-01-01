import { MapPin, Loader2, MapPinOff } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface LocationButtonProps {
  loading: boolean;
  hasLocation: boolean;
  error: string | null;
  onRequest: () => void;
}

export default function LocationButton({
  loading,
  hasLocation,
  error,
  onRequest
}: LocationButtonProps) {
  if (loading) {
    return (
      <Button variant="outline" size="sm" disabled>
        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
        Getting location...
      </Button>
    );
  }

  if (hasLocation) {
    return (
      <div className="flex items-center gap-2 text-sm text-green-600">
        <MapPin className="h-4 w-4" />
        <span>Location enabled</span>
      </div>
    );
  }

  const isPermissionDenied = error?.includes('denied');

  return (
    <div className="flex flex-col items-end gap-1">
      <Button
        onClick={onRequest}
        variant={isPermissionDenied ? "destructive" : "outline"}
        size="sm"
      >
        {isPermissionDenied ? (
          <>
            <MapPinOff className="h-4 w-4 mr-2" />
            Location Blocked
          </>
        ) : (
          <>
            <MapPin className="h-4 w-4 mr-2" />
            Use My Location
          </>
        )}
      </Button>
      {isPermissionDenied && (
        <span className="text-xs text-muted-foreground">
          Enable in browser settings
        </span>
      )}
    </div>
  );
}
