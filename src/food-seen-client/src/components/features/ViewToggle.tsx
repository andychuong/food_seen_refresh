import { List, Map } from 'lucide-react';
import { Button } from '@/components/ui/button';

export type ViewMode = 'list' | 'map';

interface ViewToggleProps {
  value: ViewMode;
  onChange: (mode: ViewMode) => void;
}

export default function ViewToggle({ value, onChange }: ViewToggleProps) {
  return (
    <div className="flex rounded-md border overflow-hidden">
      <Button
        variant={value === 'list' ? 'default' : 'ghost'}
        size="sm"
        className="rounded-none"
        onClick={() => onChange('list')}
      >
        <List className="h-4 w-4 mr-2" />
        List
      </Button>
      <Button
        variant={value === 'map' ? 'default' : 'ghost'}
        size="sm"
        className="rounded-none"
        onClick={() => onChange('map')}
      >
        <Map className="h-4 w-4 mr-2" />
        Map
      </Button>
    </div>
  );
}
