import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [TitleCasePipe],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  photos = signal<Photo[]>([]);

  ngOnInit(): void {
    this.loadPhotosForApproval()
  }

  loadPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: response => this.photos.set(response)
    });
  }

  approvePhoto(id: number) {
    this.adminService.approvePhoto(id).subscribe({
      next: () => {
        this.photos.update(prev => {
          return prev.filter(x => x.id !== id);
        })
      }
    });
  }

  rejectPhoto(id: number) {
    this.adminService.rejectPhoto(id).subscribe({
      next: () => {
        this.photos.update(prev => {
          return prev.filter(x => x.id !== id);
        })
      }
    });
  }
}
