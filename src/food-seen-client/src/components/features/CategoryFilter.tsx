import { Badge } from '@/components/ui/badge';
import type { Category } from '@/types';

interface CategoryFilterProps {
  categories: Category[];
  selected: string[];
  onChange: (selected: string[]) => void;
}

export default function CategoryFilter({
  categories,
  selected,
  onChange,
}: CategoryFilterProps) {
  const toggleCategory = (categoryId: string) => {
    if (selected.includes(categoryId)) {
      onChange(selected.filter((id) => id !== categoryId));
    } else {
      onChange([...selected, categoryId]);
    }
  };

  return (
    <div className="flex flex-wrap gap-2">
      <Badge
        variant={selected.length === 0 ? 'default' : 'outline'}
        className="cursor-pointer"
        onClick={() => onChange([])}
      >
        All
      </Badge>
      {categories.map((category) => (
        <Badge
          key={category.id}
          variant={selected.includes(category.id) ? 'default' : 'outline'}
          className="cursor-pointer"
          onClick={() => toggleCategory(category.id)}
        >
          {category.name}
        </Badge>
      ))}
    </div>
  );
}
